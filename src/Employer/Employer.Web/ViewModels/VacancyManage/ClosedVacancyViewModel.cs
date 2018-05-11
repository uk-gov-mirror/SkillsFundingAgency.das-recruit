namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ClosedVacancyViewModel : DisplayVacancyApplicationViewModel
    {
        public string ClosedDate { get; set; }

        public override bool CanShowContactDetailsHeading => base.HasContactEmail || base.HasContactName || base.HasContactTelephone;

        public override bool CanShowThingsToConsiderHeading => base.HasThingsToConsider;
    }
}
