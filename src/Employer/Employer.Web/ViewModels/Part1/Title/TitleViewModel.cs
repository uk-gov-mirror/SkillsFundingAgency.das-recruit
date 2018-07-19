﻿using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel : TitleEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(TitleEditModel.NumberOfPositions),
            nameof(TitleEditModel.Title)
        };

        public bool IsWizard { get; set; }
        public bool IsNotWizard => !IsWizard;
        public string SubmitButtonText => IsWizard ? "Save and Continue" : "Save and Preview";
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;

    }
}
