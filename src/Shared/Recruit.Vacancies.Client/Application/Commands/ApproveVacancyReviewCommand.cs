using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApproveVacancyReviewCommand : ICommand, IRequest
    {
        public Guid ReviewId { get; set; }
        public string ManualQaComment { get; set; }
        public List<ManualQaFieldIndicator> ManualQaFieldIndicators { get; set; }
        public List<Guid> SelectedAutomatedQaRuleOutcomeIds { get; set; }
    }
}
