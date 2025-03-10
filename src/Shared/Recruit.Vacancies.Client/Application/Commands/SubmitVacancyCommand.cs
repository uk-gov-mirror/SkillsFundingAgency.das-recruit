﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SubmitVacancyCommand : ICommand, IRequest
    {
        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user, OwnerType submissionOwner)
        {
            VacancyId = vacancyId;
            User = user;
            SubmissionOwner = submissionOwner;
        }

        public SubmitVacancyCommand(Guid vacancyId, VacancyUser user, string employerDescription, OwnerType submissionOwner)
        {
            VacancyId = vacancyId;
            User = user;
            EmployerDescription = employerDescription;
            SubmissionOwner = submissionOwner;
        }

        public Guid VacancyId { get;}
        public VacancyUser User { get; }
        public string EmployerDescription { get; }
        public OwnerType SubmissionOwner { get; }
    }
}
