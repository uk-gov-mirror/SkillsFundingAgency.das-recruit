using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class VacancyManageOrchestrator(
    ILogger<VacancyManageOrchestrator> logger,
    DisplayVacancyViewModelMapper vacancyDisplayMapper,
    IRecruitVacancyClient vacancyClient,
    EmployerRecruitSystemConfiguration systemConfig,
    IUtility utility)
    : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>(logger)
{
    private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
    private readonly EmployerRecruitSystemConfiguration _systemConfig = systemConfig;

    public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm, bool vacancySharedByProvider = false)
    {
        var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);

        utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId, vacancySharedByProvider);

        return vacancy;
    }

    private static Dictionary<string, string> GetRouteValues(bool vacancySharedByProvider, VacancyApplicationsPagingParams pagingParams)
    {
        var results = new Dictionary<string, string>()
        {
            { "vacancySharedByProvider", vacancySharedByProvider.ToString() }
        };
        if (pagingParams.SortColumn is not SortColumn.Default)
        {
            results.Add("sortColumn", pagingParams.SortColumn.ToString());
        }
        if (pagingParams.SortOrder is not SortOrder.Default)
        {
            results.Add("sortOrder", pagingParams.SortOrder.ToString());
        }

        return results;
    }

    public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(Vacancy vacancy, VacancyApplicationsPagingParams pagingParams, bool vacancySharedByProvider)
    {
        var viewModel = new ManageVacancyViewModel();

        viewModel.VacancyId = vacancy.Id;
        viewModel.EmployerAccountId = vacancy.EmployerAccountId;
        viewModel.Title = vacancy.Title;
        viewModel.Status = vacancy.Status;
        viewModel.VacancyReference = vacancy.VacancyReference.Value.ToString();
        viewModel.ClosingDate = viewModel.Status == VacancyStatus.Closed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate();
        viewModel.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
        viewModel.IsDisabilityConfident = vacancy.IsDisabilityConfident;
        viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
        viewModel.TransferredProviderName = vacancy.TransferInfo?.ProviderName;
        viewModel.TransferredOnDate = vacancy.TransferInfo?.TransferredDate.AsGdsDate();
        viewModel.CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates;
        viewModel.CanShowCloseVacancyLink = vacancy.CanClose;
        viewModel.CanShowDeleteLink = vacancy.CanDelete;
        viewModel.IsClosedBlockedByQa = vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.BlockedByQa;
        viewModel.CanClone = vacancy.CanClone;

        if (vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
        {
            viewModel.WithdrawnDate = vacancy.ClosedDate?.AsGdsDate();
        }

        var pagedApplicationResults = await vacancyClient.GetVacancyApplicationsAsync(pagingParams, vacancy.VacancyReference.Value, vacancySharedByProvider);
        viewModel.Applications = new VacancyApplicationsViewModel
        {
            Applications = pagedApplicationResults.Items,
            ShowDisability = vacancy.IsDisabilityConfident,
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            VacancySharedByProvier = vacancySharedByProvider
        };

        var routeValues = GetRouteValues(vacancySharedByProvider, pagingParams);
        viewModel.Applications.Pager = new PagerViewModel(
            pagedApplicationResults.TotalCount,
            pagedApplicationResults.PageSize,
            pagedApplicationResults.CurrentPage,
            "Showing {0} to {1} of {2} applications",
            RouteNames.VacancyManage_Get,
            routeValues);
        return viewModel;
    }

    public async Task<EditVacancyViewModel> GetEditVacancyViewModel(VacancyRouteModel vrm, DateTime? proposedClosingDate, DateTime? proposedStartDate)
    {
        var vacancy = await GetVacancy(vrm);

        var viewModel = new EditVacancyViewModel();
        await vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);

        if (proposedClosingDate.HasValue)
            viewModel.ProposedClosingDate = proposedClosingDate;

        if (proposedStartDate.HasValue)
            viewModel.ProposedStartDate = proposedStartDate;

        return viewModel;
    }

    public async Task<OrchestratorResponse> UpdatePublishedVacancyAsync(ProposedChangesEditModel m, VacancyUser user)
    {
        var vacancy = await GetVacancy(m);

        var proposedClosingDate = m.ProposedClosingDate.AsDateTimeUk()?.ToUniversalTime();
        var proposedStartDate = m.ProposedStartDate.AsDateTimeUk()?.ToUniversalTime();

        var updateKind = VacancyHelper.DetermineLiveUpdateKind(vacancy, proposedClosingDate, proposedStartDate);

        vacancy.ClosingDate = proposedClosingDate;
        vacancy.StartDate = proposedStartDate;
            
        return await ValidateAndExecute(
            vacancy, 
            v => vacancyClient.Validate(v, ValdationRules),
            v => vacancyClient.UpdatePublishedVacancyAsync(vacancy, user, updateKind)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel>
        {
            { e => e.StartDate, vm => vm.ProposedStartDate },
            { e => e.ClosingDate, vm => vm.ProposedClosingDate }
        };

        return mappings;
    }
}