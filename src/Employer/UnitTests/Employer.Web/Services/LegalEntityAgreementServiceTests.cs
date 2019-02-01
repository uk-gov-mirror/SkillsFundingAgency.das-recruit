using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Services
{
    public class LegalEntityAgreementServiceTests
    {
        const string EmployerAccountId = "ABCDEF";
        const long LegalEntityId = 1234;

        private Mock<IEmployerVacancyClient> _clientMock;
        private Mock<IMessaging> _messagingMock;

        [Fact]
        public void HasLegalEntityAgreementAsync_ShouldReturnFalseIfNoMatchingLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, 5678, true, 5678, true);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact]
        public void HasLegalEntityAgreementAsync_ShouldNotCheckEmployerServiceWhenHasAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, true, LegalEntityId, true);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId).Result;

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Never);
        }

        [Fact] public void HasLegalEntityAgreementAsync_ShouldCheckEmployerServiceWhenHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, LegalEntityId, true);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId).Result;

            result.Should().BeTrue();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _messagingMock.Verify(c => c.SendCommandAsync(It.Is<SetupEmployerCommand>(a => a.EmployerAccountId == EmployerAccountId)), Times.Once);
        }

        [Fact] public void HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceLegalEntityHasNoAgreement()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, LegalEntityId, false);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _messagingMock.Verify(c => c.SendCommandAsync(It.Is<SetupEmployerCommand>(a => a.EmployerAccountId == EmployerAccountId)), Times.Never);
        }

        [Fact]
        public void HasLegalEntityAgreementAsync_ShouldReturnFalseWhenEmployerServiceCantLocateLegalEntity()
        {
            var sut = GetLegalEntityAgreementService(EmployerAccountId, LegalEntityId, false, 5678, true);

            var result = sut.HasLegalEntityAgreementAsync(EmployerAccountId, LegalEntityId).Result;

            result.Should().BeFalse();
            _clientMock.Verify(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId), Times.Once);
            _messagingMock.Verify(c => c.SendCommandAsync(It.Is<SetupEmployerCommand>(a => a.EmployerAccountId == EmployerAccountId)), Times.Never);
        }

        private LegalEntityAgreementService GetLegalEntityAgreementService(string employerAccountId, long legalEntityId, bool hasLegalEntityAgreement, long employerServiceLegalEntityId, bool employerServiceHasLegalEntityAgreement)
        {
            _clientMock = new Mock<IEmployerVacancyClient>();
            _clientMock.Setup(c => c.GetEditVacancyInfoAsync(employerAccountId)).Returns(Task.FromResult(
                new EditVacancyInfo
                {
                    LegalEntities = new List<LegalEntity>
                    {
                        new LegalEntity{LegalEntityId = legalEntityId, HasLegalEntityAgreement = hasLegalEntityAgreement}
                    }
                }));

            _clientMock.Setup(c => c.GetEmployerLegalEntitiesAsync(EmployerAccountId)).Returns(Task.FromResult(
                new List<LegalEntity>
                {
                    new LegalEntity
                    {
                    LegalEntityId = employerServiceLegalEntityId,
                    HasLegalEntityAgreement = employerServiceHasLegalEntityAgreement
                    }
                }
                .AsEnumerable()));

            _messagingMock = new Mock<IMessaging>();
            _messagingMock.Setup(x => x.SendCommandAsync(It.IsAny<SetupEmployerCommand>())).Returns(Task.CompletedTask);

            return new LegalEntityAgreementService(_clientMock.Object, _messagingMock.Object);
        }
    }
}
