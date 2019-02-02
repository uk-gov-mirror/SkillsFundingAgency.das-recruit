using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class ApplicationSubmittedHandler : DomainEventHandler,  IDomainEventHandler<ApplicationSubmittedEvent>
    {
        private readonly ILogger<ApplicationSubmittedHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public ApplicationSubmittedHandler(
            ILogger<ApplicationSubmittedHandler> logger, 
            IJobsVacancyClient client,
            IMessaging messaging) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationSubmittedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);

                await _messaging.SendCommandAsync(new CreateApplicationReviewCommand { Application = @event.Application });

                _logger.LogInformation($"Finished Processing {nameof(ApplicationSubmittedEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.Application.VacancyReference, @event.Application.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

