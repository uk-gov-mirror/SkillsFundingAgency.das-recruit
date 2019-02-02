using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class LevyDeclarationOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IMessaging _messaging;

        public LevyDeclarationOrchestrator(IEmployerVacancyClient client, IMessaging messaging)
        {
            _client = client;
            _messaging = messaging;
        }

        public async Task<LevySelectionOrchestratorResponse> SaveSelectionAsync(LevyDeclarationModel viewModel, ClaimsPrincipal user)
        {
            if (viewModel.ConfirmAsLevyPayer.Value)
                await _messaging.SendCommandAsync(new SaveUserLevyDeclarationCommand
                {
                    UserId = user.GetUserId(),
                    EmployerAccountId = viewModel.EmployerAccountId
                });

            return new LevySelectionOrchestratorResponse 
            {
                RedirectRouteName = viewModel.ConfirmAsLevyPayer.Value ? RouteNames.Dashboard_Index_Get : RouteNames.NonLevyInfo_Get,
                CreateLevyCookie = viewModel.ConfirmAsLevyPayer.Value
            };
        }
    }

    public class LevySelectionOrchestratorResponse
    {
        public string RedirectRouteName { get; set; }
        public bool CreateLevyCookie { get; set; }
    }

}