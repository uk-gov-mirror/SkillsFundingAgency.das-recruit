﻿using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Wage
    {
        public int? Duration { get; set; }

        public DurationUnit? DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        public decimal? WeeklyHours { get; set; }

        public WageType? WageType { get; set; }

        public decimal? FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }
    }
}
