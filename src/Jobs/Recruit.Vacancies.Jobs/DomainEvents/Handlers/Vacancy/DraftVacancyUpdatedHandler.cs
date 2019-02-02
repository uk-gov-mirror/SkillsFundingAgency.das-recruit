using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy
{
    public class DraftVacancyUpdatedHandler : DomainEventHandler,  IDomainEventHandler<DraftVacancyUpdatedEvent>
    {
        private readonly ILogger<DraftVacancyUpdatedHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IMessaging _messaging;
        private readonly IVacancyRepository _repository;

        public DraftVacancyUpdatedHandler(
            ILogger<DraftVacancyUpdatedHandler> logger, 
            IJobsVacancyClient client,
            IMessaging messaging,
            IVacancyRepository repository) : base(logger)
        {
            _logger = logger;
            _client = client;
            _messaging = messaging;
            _repository = repository;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<DraftVacancyUpdatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(DraftVacancyUpdatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);

                await _messaging.SendCommandAsync(new AssignVacancyNumberCommand
                {
                    VacancyId = @event.VacancyId
                });

                await _messaging.SendCommandAsync(new PatchVacancyTrainingProviderCommand(@event.VacancyId));

                await EnsureVacancyIsGeocodedAsync(@event.VacancyId);

                _logger.LogInformation($"Finished Processing {nameof(DraftVacancyUpdatedEvent)} for vacancy: {{VacancyId}}", @event.VacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }

        private async Task EnsureVacancyIsGeocodedAsync(Guid vacancyId)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);

            if (!string.IsNullOrEmpty(vacancy?.EmployerLocation?.Postcode) &&
                vacancy.EmployerLocation?.HasGeocode == false)
            {
                await _messaging.SendCommandAsync(new GeocodeVacancyCommand()
                {
                    VacancyId = vacancy.Id
                });
            }
        }
    }
}