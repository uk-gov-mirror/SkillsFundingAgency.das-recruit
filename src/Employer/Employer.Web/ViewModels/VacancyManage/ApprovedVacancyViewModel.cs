namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApprovedVacancyViewModel : DisplayVacancyViewModel
    {
        public string ApprovedDate { get; set; }

        public override bool CanShowContactDetailsHeading => base.HasContactEmail || base.HasContactName || base.HasContactTelephone;

        public override bool CanShowThingsToConsiderHeading => base.HasThingsToConsider;
    }
}
