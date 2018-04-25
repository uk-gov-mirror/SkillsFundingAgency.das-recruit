using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class ApprenticeshipSummaryMapper
    {
        private const string FixedWageTypeName = "FixedWage";
        private const string FrameworkCategoryPrefix = "FW_";
        private const string StandardCategoryPrefix = "STD_";

        public static ApprenticeshipSummary MapFrom(LiveVacancy vacancy, IEnumerable<IApprenticeshipProgramme> programmes, IEnumerable<TrainingProgrammeCategory> categories)
        {
            var isFramework = vacancy.TrainingType.Equals("Framework");
            var category = GetCategory(vacancy, categories, isFramework);
            var programme = programmes.Single(p => p.Id.Equals(vacancy.ProgrammeId));

            var summary = new ApprenticeshipSummary
            {
                Title = vacancy.Title,
                StartDate = vacancy.StartDate,
                ClosingDate = vacancy.ClosingDate,
                PostedDate = vacancy.LiveDate,
                EmployerName = vacancy.EmployerName,
                Description = vacancy.Description,
                NumberOfPositions = vacancy.NumberOfPositions,
                Location =  new ApprenticeshipSummary.GeoPoint
                                {
                                    Lat = vacancy.EmployerLocation.Latitude,
                                    Lon = vacancy.EmployerLocation.Longitude
                                },
                ApprenticeshipLevel = programme.Level.ToString(),
                VacancyReference = vacancy.VacancyReference.ToString(),
                Category = category.Category,
                CategoryCode = category.CategoryCode,
                SubCategory = category.SubCategory,
                SubCategoryCode = category.SubCategoryCode,
                WageType = GetLegacyWageType(vacancy.Wage.WageType),
                WageAmount = vacancy.Wage.FixedWageYearlyAmount,
                HoursPerWeek = vacancy.Wage.WeeklyHours,
            };

            if (isFramework)
            {
                summary.FrameworkLarsCode = GetFrameworkGroupCode(vacancy.ProgrammeId);
            }
            else
            {
                summary.StandardLarsCode = int.Parse(vacancy.ProgrammeId);
            }

            if (vacancy.Wage.WageType.Equals(FixedWageTypeName))
            {
                summary.WageUnit = 4; // Yearly
            }

            return summary;
        }

        private static TrainingProgrammeCategory GetCategory(LiveVacancy vacancy, IEnumerable<TrainingProgrammeCategory> categories, bool isFramework)
        {
            var categoryIdentifier = isFramework 
                                ? $"{FrameworkCategoryPrefix}{GetFrameworkGroupCode(vacancy.ProgrammeId)}" 
                                : $"{StandardCategoryPrefix}{vacancy.ProgrammeId}";
            var category = categories.Single(tpc => tpc.Id.Equals(categoryIdentifier));
            return category;
        }

        private static int GetLegacyWageType(string wageType)
        {
            switch (wageType)
            {
                case FixedWageTypeName:
                    return 4;
                case "NationalMinimumWageForApprentices":
                    return 2;
                case "NationalMinimumWage":
                    return 3;
                case "Unspecified":
                    return 8;
                default:
                    return 4;
            }
        }

        private static string GetFrameworkGroupCode(string programmeId)
        {
            const char seperator = '-';

            if (programmeId.Contains(seperator))
            {
                var indexOfSeperator = programmeId.IndexOf(seperator);
                return programmeId.Substring(0, indexOfSeperator);
            }

            return programmeId;
        }
    }
}
