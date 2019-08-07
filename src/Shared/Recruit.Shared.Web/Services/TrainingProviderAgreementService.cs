﻿using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;

namespace Esfa.Recruit.Shared.Web.Services
{
    public class TrainingProviderAgreementService : ITrainingProviderAgreementService
    {
        private readonly IProviderVacancyClient _client;
        private readonly IPasAccountClient _pasAccountClient;

        public TrainingProviderAgreementService(IProviderVacancyClient client, IPasAccountClient pasAccountClient)
        {
            _client = client;
            _pasAccountClient = pasAccountClient;
        }

        public async Task<bool> HasAgreementAsync(long ukprn)
        {
            var editVacancyInfo = await _client.GetProviderEditVacancyInfoAsync(ukprn);

            if (editVacancyInfo == null)
                return false;

            if (editVacancyInfo.HasProviderAgreement)
                return true;

            //Agreement may have been signed since the projection was created. Check PAS.
            var hasAgreement = await _pasAccountClient.HasAgreementAsync(ukprn);

            if (hasAgreement)
            {
                await _client.SetupProviderAsync(ukprn);
            }

            return hasAgreement;
        }
    }
}
