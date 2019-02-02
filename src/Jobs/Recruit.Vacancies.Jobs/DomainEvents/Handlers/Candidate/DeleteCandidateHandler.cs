using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Candidate
{
    public class DeleteCandidateHandler : DomainEventHandler, IDomainEventHandler<CandidateDeleteEvent>
    {
        private readonly ILogger<DeleteCandidateHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public DeleteCandidateHandler(
            ILogger<DeleteCandidateHandler> logger, 
            IJobsVacancyClient client,
            IMessaging messaging) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<CandidateDeleteEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(CandidateDeleteEvent)} for candidate: {{CandidateId}}", @event.CandidateId);

                await _messaging.SendCommandAsync(new DeleteApplicationReviewsCommand
                {
                    CandidateId = @event.CandidateId
                });
                
                _logger.LogInformation($"Finished Processing {nameof(CandidateDeleteEvent)} for vacancy: candidate: {{CandidateId}}", @event.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }

        }
    }
}
