﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Reports
{
    public class ProviderApplicationsReportOrchestratorTests
    {
        private readonly Mock<IProviderVacancyClient> _client = new Mock<IProviderVacancyClient>();

        [Theory]
        [InlineData(DateRangeType.Last7Days, "2019-02-26", "2019-03-05")]
        [InlineData(DateRangeType.Last14Days, "2019-02-19", "2019-03-05")]
        [InlineData(DateRangeType.Last30Days, "2019-02-03", "2019-03-05")]
        public async Task PostCreateViewModelAsync_ShouldUseCorrectTimespan(DateRangeType dateRangeType, string fromDate, string toDate)
        {
            var orchestrator = GetOrchestrator();

            long ukprn = 12345678;
            string reportName = $"{DateTime.Parse(fromDate).AsGdsDate()} to {DateTime.Parse(toDate).AsGdsDate()}";

            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
                DateRange = dateRangeType
            };

            var user = new VacancyUser();

            await orchestrator.PostCreateViewModelAsync(model, user);

            _client.Verify(c => c.CreateProviderApplicationsReportAsync(
                ukprn, 
                DateTime.Parse(fromDate), 
                DateTime.Parse(toDate).AddDays(1).AddTicks(-1), 
                user, 
                reportName), 
                Times.Once);
        }

        [Fact]
        public async Task PostCreateViewModelAsync_ShouldUseCustomTimespan()
        {
            long ukprn = 12345678;

            var model = new ProviderApplicationsReportCreateEditModel {
                Ukprn = ukprn,
                DateRange = DateRangeType.Custom,
                FromDay = "1",
                FromMonth = "2",
                FromYear = "2018",
                ToDay = "3",
                ToMonth = "4",
                ToYear = "2019",
            };

            var user = new VacancyUser();

            var orchestrator = GetOrchestrator();

            await orchestrator.PostCreateViewModelAsync(model, user);

            _client.Verify(c => c.CreateProviderApplicationsReportAsync(
                    ukprn,
                    DateTime.Parse("2018-02-01").ToUniversalTime(),
                    DateTime.Parse("2019-04-03").AddDays(1).AddTicks(-1).ToUniversalTime(), 
                    user,
                    "01 Feb 2018 to 03 Apr 2019"),
                Times.Once);
        }

        private ProviderApplicationsReportOrchestrator GetOrchestrator()
        {
            var timeProvider = new Mock<ITimeProvider>();

            var today = DateTime.Parse("2019-03-05").ToUniversalTime();
            timeProvider.Setup(t => t.Today).Returns(today);
            timeProvider.Setup(t => t.NextDay).Returns(today.AddDays(1));

            return new ProviderApplicationsReportOrchestrator(_client.Object, timeProvider.Object);
        }
    }
}
