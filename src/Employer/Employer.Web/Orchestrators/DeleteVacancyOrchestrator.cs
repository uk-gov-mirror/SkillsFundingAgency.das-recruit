using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IMessaging _messaging;

        public DeleteVacancyOrchestrator(
            IEmployerVacancyClient client, 
            IRecruitVacancyClient vacancyClient,
            IMessaging messaging)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _messaging = messaging;
        }

        public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new DeleteViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task DeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(m.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            await _messaging.SendCommandAsync(new DeleteVacancyCommand
            {
                VacancyId = vacancy.Id,
                User = user
            });
        }
    }
}
