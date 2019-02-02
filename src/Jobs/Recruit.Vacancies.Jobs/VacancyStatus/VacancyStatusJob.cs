using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyStatus
{
    public class VacancyStatusJob
    {
#if DEBUG
        private const bool CanRunOnStartup = true;
#else
        private const bool CanRunOnStartup = false;
#endif
        private readonly ILogger<VacancyStatusJob> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public VacancyStatusJob(
            ILogger<VacancyStatusJob> logger, 
            RecruitWebJobsSystemConfiguration jobsConfig, 
            IJobsVacancyClient client,
            IMessaging messaging)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _client = client;
            _messaging = messaging;
        }

        public async Task Run([TimerTrigger(Schedules.MidnightDaily, RunOnStartup = CanRunOnStartup)] TimerInfo timerInfo, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting vacancy status checking.");

            try
            {
                await _messaging.SendCommandAsync(new CloseExpiredVacanciesCommand());
                _logger.LogInformation("Finished vacancy status checking.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check vacancy statuses.");
                throw;
            }
        }
    }
}