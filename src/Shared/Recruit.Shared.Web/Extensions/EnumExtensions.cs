using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            DisplayNames.TryGetValue(enumValue, out var displayName);
            return displayName ?? enumValue.ToString();
        }

        public static bool IsInLiveVacancyOptions(this FilteringOptions enumValue)
        {
            return enumValue == FilteringOptions.ClosingSoon ||
                   enumValue == FilteringOptions.ClosingSoonWithNoApplications;
        }

        private static readonly Dictionary<Enum, string> DisplayNames = new Dictionary<Enum, string>
        {
            { WageType.FixedWage, "Fixed wage" },
            { WageType.NationalMinimumWage, "National Minimum Wage" },
            { WageType.NationalMinimumWageForApprentices, "National Minimum Wage for apprentices" },
            { ManualQaOutcome.Referred, "Edits required" },
            { ReviewStatus.UnderReview, "Under review" },
            { VacancyStatus.Referred, "Rejected" },
            { VacancyStatus.Submitted, "Pending review" },                     
            { ApplicationReviewDisabilityStatus.PreferNotToSay, "Prefer not to say" },
            { FilteringOptions.ClosingSoon, "closing soon" },
            { FilteringOptions.ClosingSoonWithNoApplications, "closing soon without applications" },
            { FilteringOptions.AllApplications, "with applications" },
            { FilteringOptions.NewApplications, "with new applications" },
            { FilteringOptions.Submitted, "Pending review" },
            { FilteringOptions.Referred, "Rejected" },
            { FilteringOptions.Transferred, "Transferred from provider" }
        };
    }
}