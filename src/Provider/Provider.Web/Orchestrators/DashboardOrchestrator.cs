﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private readonly IProviderVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitVacancyClient _client;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;

        public DashboardOrchestrator(
            IProviderVacancyClient vacancyClient,
            ITimeProvider timeProvider,
            IRecruitVacancyClient client,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            var dashboardTask = _vacancyClient.GetDashboardAsync(user.Ukprn.Value, createIfNonExistent: true);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(dashboardTask, userDetailsTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;

            var vacancies = dashboard.Vacancies?.ToList() ?? new List<VacancySummary>();

            var vm = new DashboardViewModel
            {
                Vacancies = vacancies,
                NoOfVacanciesClosingSoonWithNoApplications = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live &&
                    v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                    v.NoOfApplications == 0),
                NoOfVacanciesClosingSoon = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live),
                Alerts = _providerAlertsViewModelFactory.Create(dashboard, userDetails)
            };
            return vm;
        }
    }
}