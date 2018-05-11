namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SubmittedVacancyViewModel : DisplayVacancyViewModel
    {
        public string SubmittedDate { get; set; }

        public override bool CanShowContactDetailsHeading => base.HasContactEmail || base.HasContactName || base.HasContactTelephone;

        public override bool CanShowThingsToConsiderHeading => base.HasThingsToConsider;
    }
}
