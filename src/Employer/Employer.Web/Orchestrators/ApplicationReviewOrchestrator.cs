﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ApplicationReviewOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IMessaging _messaging;

        public ApplicationReviewOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, IMessaging messaging)
        {
            _messaging = messaging;
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewRouteModel rm)
        {
            var applicationReview = await Utility.GetAuthorisedApplicationReviewAsync(_vacancyClient, rm);

            if (applicationReview.IsWithdrawn)
                throw new Exception($"Application has been withdrawn. ApplicationReviewId:{applicationReview.Id}");

            return applicationReview.ToViewModel();
        }

        public async Task<ApplicationReviewViewModel> GetApplicationReviewViewModelAsync(ApplicationReviewEditModel m)
        {
            var vm = await GetApplicationReviewViewModelAsync((ApplicationReviewRouteModel)m);

            vm.Outcome = m.Outcome;
            vm.CandidateFeedback = m.CandidateFeedback;

            return vm;
        }

        public Task PostApplicationReviewEditModelAsync(ApplicationReviewEditModel m, VacancyUser user)
        {
            switch (m.Outcome.Value)
            {
                case ApplicationReviewStatus.Successful:
                    return _messaging.SendCommandAsync(new ApplicationReviewSuccessfulCommand
                    {
                        ApplicationReviewId = m.ApplicationReviewId,
                        User = user
                    });
                case ApplicationReviewStatus.Unsuccessful:
                    return _messaging.SendCommandAsync(new ApplicationReviewUnsuccessfulCommand
                    {
                        ApplicationReviewId = m.ApplicationReviewId,
                        CandidateFeedback = m.CandidateFeedback,
                        User = user
                    });
                default:
                    throw new ArgumentException("Unhandled ApplicationReviewStatus");
            }
        }
    }
}
