using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancySubmittedHandler : DomainEventHandler, IDomainEventHandler<VacancySubmittedEvent>
    {
        private readonly ILogger<VacancySubmittedHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public VacancySubmittedHandler(
            ILogger<VacancySubmittedHandler> logger, 
            IJobsVacancyClient client,
            IMessaging messaging) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancySubmittedEvent>(eventPayload);
            
            try
            {
                _logger.LogInformation($"Processing {nameof(VacancySubmittedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
                
                await _messaging.SendCommandAsync(new CreateVacancyReviewCommand
                {
                    VacancyReference = @event.VacancyReference
                });
                
                _logger.LogInformation($"Finished Processing {nameof(VacancySubmittedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

