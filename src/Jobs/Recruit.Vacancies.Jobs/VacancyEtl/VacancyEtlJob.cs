using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class VacancyEtlJob
    {
        private readonly ILogger<VacancyEtlJob> _logger;
        private readonly LiveVacancyIndexHandler _handler;
        private string JobName => GetType().Name;

        public VacancyEtlJob(ILogger<VacancyEtlJob> logger, LiveVacancyIndexHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.VacancyIndexQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var indexMsg = JsonConvert.DeserializeObject<IndexMessage>(message);
                _logger.LogInformation($"Start {JobName}");
                _logger.LogInformation($"Beginning indexing of live vacancies to {indexMsg.IndexName} index.");
                await _handler.Handle(indexMsg.IndexName);
                _logger.LogInformation($"Finished {JobName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
            finally
            {

                // publish service bus VacancySummaryUpdateComplete 
            }
        }
    }
}