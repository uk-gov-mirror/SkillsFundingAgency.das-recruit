using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CloneVacancyCommand : ICommand<Guid>, IRequest<Guid>
    {
        public Guid IdOfVacancyToClone { get; private set; }
        public VacancyUser User { get; private set; }
        public SourceOrigin SourceOrigin { get; private set; }

        public CloneVacancyCommand(Guid cloneVacancyId, VacancyUser user, SourceOrigin sourceOrigin)
        {
            IdOfVacancyToClone = cloneVacancyId;
            User = user;
            SourceOrigin = sourceOrigin;
        }
    }
}
