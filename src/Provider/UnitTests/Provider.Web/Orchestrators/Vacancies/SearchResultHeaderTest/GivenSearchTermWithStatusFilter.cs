using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenSearchTermWithStatusFilter : SearchResultHeaderTestBase
    {
        [Fact]
        public async Task WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 live adverts with 'nurse'";
            var sut = GetSut(new List<VacancySummary>());
            var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 live advert with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty,VacancyStatus.Live));
            var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 live adverts with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty,VacancyStatus.Live));
            var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData("2 live adverts with 'nurse'", "Live", "nurse", 2, VacancyStatus.Live)]
        [InlineData("1 live advert", "Live", "", 1, VacancyStatus.Live)]
        [InlineData("2 draft adverts with 'nurse'", "Draft", "nurse", 2, VacancyStatus.Draft)]
        [InlineData("2 adverts pending review with 'nurse'", "Submitted", "nurse", 2, VacancyStatus.Submitted)]
        [InlineData("2 rejected adverts", "Referred", "", 2, VacancyStatus.Referred)]
        [InlineData("2 closed adverts", "Closed", "", 2, VacancyStatus.Closed)]
        [InlineData("2 adverts closing soon with 'nurse'", "ClosingSoon", "nurse", 2, VacancyStatus.Live)]
        [InlineData("0 adverts closing soon without applications with 'nurse'", "ClosingSoonWithNoApplications", "nurse", 2, VacancyStatus.Live)]
        public async Task WhenThereIsMoreThanOneVacancy_WithFilters(string expectedMessage, string filter, string searchTerm, int count, VacancyStatus status)
        {
            var sut = GetSut(GenerateVacancySummaries(count, searchTerm, string.Empty, status));
            var vm = await sut.GetVacanciesViewModelAsync(User, filter, 1, searchTerm);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}