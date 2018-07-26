using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public string Title { get; internal set; }

        public List<string> QualificationTypes { get; set; }

		public bool HasUnsavedQualifications { get; internal set; }

		public bool HasQualifications => Qualifications.Any();

		public static string PreviewSectionAnchor => PreviewAnchors.RequirementsAndProspectsSection;
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationsEditModel.QualificationType),
            nameof(QualificationsEditModel.Subject),
            nameof(QualificationsEditModel.Grade),
            nameof(QualificationsEditModel.Weighting)
        };
	}
}
