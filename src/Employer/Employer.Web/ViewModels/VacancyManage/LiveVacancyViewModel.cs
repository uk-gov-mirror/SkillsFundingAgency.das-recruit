using System;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class LiveVacancyViewModel : DisplayVacancyApplicationViewModel
    {
        public string LiveDate { get; internal set; }

        public override bool CanShowContactDetailsHeading => base.HasContactEmail || base.HasContactName || base.HasContactTelephone;

        public override bool CanShowThingsToConsiderHeading => base.HasThingsToConsider;
    }
}