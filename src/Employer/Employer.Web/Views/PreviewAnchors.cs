using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Esfa.Recruit.Employer.Web.Views
{
    public static class PreviewAnchors
    {
        public const string TitlePart1Section = "titleSection";



        public const string ApprenticeshipSummarySection = "apprenticeshipSummary";
        public const string RequirementsAndProspectsSection = "reqAndProspects";
        public const string AboutEmployerSection = "aboutEmployer";
        public const string ApplicationProcessSection = "applicationProcess";
        public const string TrainingProviderSection = "trainingProvider";

        public static IEnumerable<string> GetPart1SectionAnchors()
        {
            var fields = typeof(PreviewAnchors).GetFields(BindingFlags.Public | BindingFlags.Static);

            return fields
                    .Where(f => f.FieldType == typeof(string)
                            && f.IsLiteral
                            && f.Name.EndsWith("Part1Section"))
                    .Select(f => (string)f.GetRawConstantValue());
        }
    }
}