using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammesJob
    {
        private readonly ILogger<ApprenticeshipProgrammesJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public ApprenticeshipProgrammesJob(
            ILogger<ApprenticeshipProgrammesJob> logger, 
            RecruitWebJobsSystemConfiguration jobsConfig, 
            IJobsVacancyClient client,
            IMessaging messaging)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
            _messaging = messaging;
        }

        public async Task UpdateStandardsAndFrameworks([TimerTrigger(Schedules.FourAmDaily, RunOnStartup = true)] TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting populating standards and frameworks into reference data store.");

            try
            {
                await _messaging.SendCommandAsync(new UpdateApprenticeshipProgrammesCommand());

                _logger.LogInformation("Finished populating standards and frameworks into reference data store.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update standards and frameworks.");
                throw;
            }
        }
    }
}