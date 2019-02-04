using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using System;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CloneVacancyCommandHandler: IRequestHandler<CloneVacancyCommand, Guid>
    {
        private readonly ILogger<CloneVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public CloneVacancyCommandHandler(
            ILogger<CloneVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task<Guid> Handle(CloneVacancyCommand message, CancellationToken cancellationToken)
        {
            var newVacancyId = Guid.NewGuid();

            _logger.LogInformation("Cloning new vacancy with id: {vacancyId} from vacancy with id: {clonedVacancyId}", message.IdOfVacancyToClone, newVacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.IdOfVacancyToClone);

            if (vacancy.Status != VacancyStatus.Submitted && vacancy.Status != VacancyStatus.Live && vacancy.Status != VacancyStatus.Closed)
            {
                _logger.LogError($"Unable to clone vacancy {{vacancyId}} due to it having a status of {vacancy.Status}.", message.IdOfVacancyToClone);
                
                throw new InvalidStateException($"Vacancy is not in correct state to be cloned. Current State: {vacancy.Status}");
            }

            var clone = CreateClone(newVacancyId, message, vacancy);

            await _repository.CreateAsync(clone);

            await _messaging.PublishEvent(new VacancyClonedEvent
            {
                VacancyId = clone.Id
            });

            return newVacancyId;
        }

        private Vacancy CreateClone(Guid newVacancyId, CloneVacancyCommand message, Vacancy vacancy)
        {
            var now = _timeProvider.Now;

            var clone = JsonConvert.DeserializeObject<Vacancy>(JsonConvert.SerializeObject(vacancy));

            // Properties to replace
            clone.Id = newVacancyId;
            clone.CreatedByUser = message.User;
            clone.CreatedDate = now;
            clone.LastUpdatedByUser = message.User;
            clone.LastUpdatedDate = now;
            clone.SourceOrigin = message.SourceOrigin;
            clone.SourceType = SourceType.Clone;
            clone.SourceVacancyReference = vacancy.VacancyReference;
            clone.Status = VacancyStatus.Draft;
            clone.IsDeleted = false;

            // Properties to remove
            clone.VacancyReference = null;
            clone.ApprovedDate = null;
            clone.ClosedDate = null;
            clone.DeletedByUser = null;
            clone.DeletedDate = null;
            clone.LiveDate = null;
            clone.SubmittedByUser = null;
            clone.SubmittedDate = null;

            return clone;
        }
    }
}
