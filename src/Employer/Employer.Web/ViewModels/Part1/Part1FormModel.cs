using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1
{
    public abstract class Part1FormModel : VacancyRouteModel
    {
        public Part1FormModel()
        {
            CanCancelButtonReturnToDashboard = true;
        }

        public bool CanCancelButtonReturnToDashboard { get; internal set; }
        public bool CanCancelButtonReturnToPreview => !CanCancelButtonReturnToDashboard;
        public bool CanShowStepCaption => CanCancelButtonReturnToDashboard;
        public bool CanSubmitProgressToPreview => CanCancelButtonReturnToPreview;
        public string SubmitButtonText => CanCancelButtonReturnToDashboard ? "Save and continue" : "Save and preview";

        [FromForm]
        public bool CanProgressToPreview { get; set; }
    }
}
