using System.Collections.Generic;
using AutoFixture;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.VacancyClientTests;

public class GetVacancyApplicationsAsyncTests
{
    [Test, MoqAutoData]
    public async Task Returns_No_Results(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        applicationReviewRepository.Setup(x => x.GetCountForVacancyAsync(It.IsAny<long>())).ReturnsAsync(0);
        
        // act
        var result = await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, false);

        // assert
        applicationReviewRepository.Verify(x => x.GetCountForSharedVacancyAsync(vacancyReference), Times.Never);
        applicationReviewRepository.Verify(x => x.GetCountForVacancyAsync(vacancyReference), Times.Once);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(pagingParams.PageSize);
    }
    
    [Test, MoqAutoData]
    public async Task Returns_No_Results_For_Shared_Vacancy(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        applicationReviewRepository.Setup(x => x.GetCountForSharedVacancyAsync(It.IsAny<long>())).ReturnsAsync(0);
        
        // act
        var result = await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, true);

        // assert
        applicationReviewRepository.Verify(x => x.GetCountForSharedVacancyAsync(vacancyReference), Times.Once);
        applicationReviewRepository.Verify(x => x.GetCountForVacancyAsync(vacancyReference), Times.Never);
        
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(pagingParams.PageSize);
    }
    
    [Test, MoqAutoData]
    public async Task Returns_Paged_Results(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        pagingParams = pagingParams with
        {
            PageNumber = 2,
            PageSize = 10
        };
        var fixture = new Fixture { RepeatCount = 10 };
        var vacancyApplications = fixture.Create<List<ApplicationReview>>();
        
        applicationReviewRepository.Setup(x => x.GetCountForVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancyApplications.Count*2);
        applicationReviewRepository
            .Setup(x => x.GetForVacancyAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(vacancyApplications);
        
        // act
        var result = await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, false);

        // assert
        applicationReviewRepository.Verify(x => x.GetForVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, 10, pagingParams.PageSize), Times.Once);
        applicationReviewRepository.Verify(x => x.GetForSharedVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, 10, pagingParams.PageSize), Times.Never);
        
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(20);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(pagingParams.PageSize);
    }
    
    [Test, MoqAutoData]
    public async Task Returns_Paged_Results_For_Shared_Vacancy(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        pagingParams = pagingParams with
        {
            PageNumber = 2,
            PageSize = 10
        };
        var fixture = new Fixture { RepeatCount = 10 };
        var vacancyApplications = fixture.Create<List<ApplicationReview>>();
        
        applicationReviewRepository.Setup(x => x.GetCountForSharedVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancyApplications.Count*2);
        applicationReviewRepository
            .Setup(x => x.GetForSharedVacancyAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(vacancyApplications);
        
        // act
        var result = await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, true);

        // assert
        applicationReviewRepository.Verify(x => x.GetForSharedVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, 10, pagingParams.PageSize), Times.Once);
        applicationReviewRepository.Verify(x => x.GetForVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, 10, pagingParams.PageSize), Times.Never);
        
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(20);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(pagingParams.PageSize);
    }
    
    [Test, MoqAutoData]
    public async Task Requested_Page_Is_Clamped_To_Lower_Bounds(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        const int expectedSkipCount = 0;
        pagingParams = pagingParams with
        {
            PageNumber = -1,
            PageSize = 10
        };
        var fixture = new Fixture { RepeatCount = 10 };
        var vacancyApplications = fixture.Create<List<ApplicationReview>>();
        
        applicationReviewRepository.Setup(x => x.GetCountForVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancyApplications.Count*2);
        applicationReviewRepository
            .Setup(x => x.GetForVacancyAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(vacancyApplications);
        
        // act
        await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, false);

        // assert
        applicationReviewRepository.Verify(x => x.GetForVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, expectedSkipCount, pagingParams.PageSize), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Requested_Page_Is_Clamped_To_Upper_Bounds(
        VacancyApplicationsPagingParams pagingParams,
        long vacancyReference,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
        [Greedy] VacancyClient sut)
    {
        // arrange
        const int expectedSkipCount = 10;
        pagingParams = pagingParams with
        {
            PageNumber = 100,
            PageSize = 10
        };
        var fixture = new Fixture { RepeatCount = 10 };
        var vacancyApplications = fixture.Create<List<ApplicationReview>>();
        
        applicationReviewRepository.Setup(x => x.GetCountForVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancyApplications.Count*2);
        applicationReviewRepository
            .Setup(x => x.GetForVacancyAsync(It.IsAny<long>(), It.IsAny<SortColumn>(), It.IsAny<SortOrder>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(vacancyApplications);
        
        // act
        await sut.GetVacancyApplicationsAsync(pagingParams, vacancyReference, false);

        // assert
        applicationReviewRepository.Verify(x => x.GetForVacancyAsync(vacancyReference, pagingParams.SortColumn, pagingParams.SortOrder, expectedSkipCount, pagingParams.PageSize), Times.Once);
    }
}