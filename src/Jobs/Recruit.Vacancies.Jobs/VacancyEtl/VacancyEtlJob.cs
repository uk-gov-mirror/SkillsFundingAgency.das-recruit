using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class VacancyEtlJob
    {
        private readonly ILogger<VacancyEtlJob> _logger;
        private readonly LiveVacancyIndexHandler _handler;
        private readonly ITopicClient _topicClient;

        private string JobName => GetType().Name;

        public VacancyEtlJob(ILogger<VacancyEtlJob> logger, LiveVacancyIndexHandler handler)
        {
            _logger = logger;
            _handler = handler;
            _topicClient = new TopicClient("", "VacancyIndexCreated");
        }

        public async Task HandleVacancyEvent([QueueTrigger(QueueNames.VacancyIndexQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            DateTime? scheduledRefreshDateTime = null;

            try
            {
                var indexMsg = JsonConvert.DeserializeObject<IndexMessage>(message);
                _logger.LogInformation($"Start {JobName}");
                _logger.LogInformation($"Beginning indexing of live vacancies to {indexMsg.IndexName} index.");
                scheduledRefreshDateTime = indexMsg.ScheduledRefreshDateTime;
                await _handler.Handle(indexMsg.IndexName);
                _logger.LogInformation($"Finished {JobName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
            }
            finally
            {
                if (scheduledRefreshDateTime.HasValue)
                {
                    _logger.LogInformation("Publishing VacancySummaryUpdateComplete message to queue");

                    var vsuc = new VacancySummaryUpdateComplete
                    {
                        ScheduledRefreshDateTime = scheduledRefreshDateTime.Value
                    };
                    _topicClient.SendAsync(new Message())
                    //_serviceBus.PublishMessage(vsuc);

                    _logger.LogInformation("Published VacancySummaryUpdateComplete message published to queue");
                }
            }
        }
    }

    internal class VacancySummaryUpdateComplete
    {
        public DateTime ScheduledRefreshDateTime { get; set; }
    }
}