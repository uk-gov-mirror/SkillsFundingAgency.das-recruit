using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class ApplicationWithdrawnHandler : DomainEventHandler, IDomainEventHandler<ApplicationWithdrawnEvent>
    {
        private readonly ILogger<ApplicationWithdrawnEvent> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public ApplicationWithdrawnHandler(
            ILogger<ApplicationWithdrawnEvent> logger, 
            IJobsVacancyClient client,
            IMessaging messaging) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationWithdrawnEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);

                await _messaging.SendCommandAsync(new WithdrawApplicationCommand
                {
                    VacancyReference = @event.VacancyReference,
                    CandidateId = @event.CandidateId
                });

                _logger.LogInformation($"Finished Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}
