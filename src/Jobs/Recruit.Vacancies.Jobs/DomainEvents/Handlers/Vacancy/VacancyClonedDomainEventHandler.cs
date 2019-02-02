using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class VacancyClonedDomainEventHandler : DomainEventHandler, IDomainEventHandler<VacancyClonedEvent>
    {
        private readonly ILogger<VacancyClonedDomainEventHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;

        public VacancyClonedDomainEventHandler(
            ILogger<VacancyClonedDomainEventHandler> logger, 
            IJobsVacancyClient client,
            IMessaging messaging) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyClonedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyClonedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
                
                await _messaging.SendCommandAsync(new AssignVacancyNumberCommand
                {
                    VacancyId = @event.VacancyId
                });
                
                _logger.LogInformation($"Finished Processing {nameof(VacancyClonedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

