using System;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class ApprenticeshipSummary
    {
        public string Id => VacancyReference;
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime PostedDate { get; set; }
        public string EmployerName { get; set; }
        public string Description { get; set; }
        public int NumberOfPositions { get; set; }
        public string VacancyLocationType => "NonNational";
        public GeoPoint Location { get; set; }
        public string ApprenticeshipLevel { get; set; }
        public string VacancyReference { get; set; }
        public string Category { get; set; }
        public string CategoryCode { get; set; }
        public string SubCategory { get; set; }
        public string SubCategoryCode { get; set; }
        public int WageType { get; set; }
        public int? WageUnit { get; set; }
        public decimal? WageAmount { get; set; }
        public decimal HoursPerWeek { get; set; }
        public int? StandardLarsCode { get; set; }
        public string FrameworkLarsCode { get; set; }
        public bool IsDisabilityConfident => false;

        public class GeoPoint
        {
            public double Lon { get; set; }
            public double Lat { get; set; }
        }
    }
}
