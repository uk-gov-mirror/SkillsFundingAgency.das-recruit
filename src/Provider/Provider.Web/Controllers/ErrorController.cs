﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Error;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Providers.Api.Client;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IRecruitVacancyClient _vacancyClient;

        public ErrorController(ILogger<ErrorController> logger, IOptions<ExternalLinksConfiguration> externalLinks, IRecruitVacancyClient vacancyClient)
        {
            _logger = logger;
            _externalLinks = externalLinks.Value;
            _vacancyClient = vacancyClient;
        }

        [Route("error/{id?}")]
        public IActionResult Error(int id)
        {
            ViewBag.IsErrorPage = true; // Used by layout to show/hide elements.

            switch (id)
            {
                case 403:
                    return AccessDenied();
                case 404:
                    return PageNotFound();
                default:
                    break;
            }

            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route(RoutePaths.ExceptionHandlingPath)]
        public IActionResult ErrorHandler()
        {
            if (HttpContext.Items.TryGetValue(ContextItemKeys.ProviderIdentifier, out var ukprn))
            {
                if (!RouteData.Values.ContainsKey(ContextItemKeys.ProviderIdentifier))
                    RouteData.Values.Add(ContextItemKeys.ProviderIdentifier, ukprn);
            }

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                string routeWhereExceptionOccurred = exceptionFeature.Path;
                var exception = exceptionFeature.Error;

                if (exception is AggregateException aggregateException)
                {
                    var flattenedExceptions = aggregateException.Flatten();
                    _logger.LogError(flattenedExceptions, "Aggregate exception on path: {route}", routeWhereExceptionOccurred);

                    exception = flattenedExceptions.InnerExceptions.FirstOrDefault();
                }

                if (exception is InvalidStateException)
                {
                    _logger.LogWarning(exception, "Exception on path: {route}", routeWhereExceptionOccurred);
                    AddDashboardMessage(exception.Message);
                    return RedirectToRoute(RouteNames.Vacancies_Get, new { Ukprn = ukprn });
                }

                if (exception is InvalidRouteForVacancyException invalidRouteException)
                {
                    return RedirectToRoute(invalidRouteException.RouteNameToRedirectTo, invalidRouteException.RouteValues);
                }

                if (exception is VacancyNotFoundException)
                {
                    return PageNotFound();
                }

                if (exception is AuthorisationException)
                {
                    return AccessDenied();
                }

                if (exception is ReportNotFoundException)
                {
                    return PageNotFound();
                }

                if (exception is MissingPermissionsException mpEx)
                {
                    _logger.LogInformation(mpEx.Message);
                    return MissingPermissions(long.Parse((string)ukprn));
                }

                _logger.LogError(exception, "Unhandled exception on path: {route}", routeWhereExceptionOccurred);
            }

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View(ViewNames.ErrorView, new ErrorViewModel { StatusCode = (int)HttpStatusCode.InternalServerError, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IActionResult MissingPermissions(long ukprn)
        {
            var vm = new MissingPermissionsViewModel
            {
                RouteValues = new VacancyRouteModel { Ukprn = ukprn },
                CtaRoute = RouteNames.Vacancies_Get
            };

            return View(ViewNames.MissingPermissions, vm);
        }

        private IActionResult AccessDenied()
        {
            if (TempData.ContainsKey(TempDataKeys.IsBlockedProvider) && (bool)TempData.Peek(TempDataKeys.IsBlockedProvider))
            {
                return ProviderAccessRevoked();
            }

            var serviceClaims = User.FindAll(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier);

            if (serviceClaims.Any(claim => claim.Value.Equals(ProviderRecruitClaims.ServiceClaimValue) == false))
            {
                _logger.LogInformation("User does not have service claim.");
                return Redirect(_externalLinks.ProviderApprenticeshipSiteUrl);
            }

            var ukprnClaim = User.FindFirst(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier);
            if (!string.IsNullOrEmpty(ukprnClaim.Value))
            {
                var ukprn = long.Parse(ukprnClaim.Value);
                bool ukprnIsNotListedInRoatp;
                try
                {
                    var allProviders = _vacancyClient.GetAllTrainingProvidersAsync().Result;
                    var provider = allProviders.SingleOrDefault(p => p.Ukprn == ukprn);
                    ukprnIsNotListedInRoatp = provider == null;
                }
                catch (Exception)
                {
                    ukprnIsNotListedInRoatp = true;
                }

                if (ukprnIsNotListedInRoatp)
                {
                    _logger.LogInformation($"Provider {ukprn} is not listed in RoATP for user {User.Identity.Name}.");
                    return Redirect(_externalLinks.ProviderApprenticeshipSiteUrl);
                }
            }

            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(ViewNames.AccessDenied);
        }

        private IActionResult ProviderAccessRevoked()
        {
            var vm = new ProviderAccessRevokedViewModel
            {
                Ukprn = (string)TempData[TempDataKeys.ProviderIdentifier],
                ProviderName = (string)TempData[TempDataKeys.ProviderName],
            };

            TempData.Keep();
            return View(ViewNames.ProviderAccessRevoked, vm);
        }

        private IActionResult PageNotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View(ViewNames.PageNotFound);
        }

        private void AddDashboardMessage(string message)
        {
            if(TempData.ContainsKey(TempDataKeys.VacanciesErrorMessage))
                _logger.LogError($"Dashboard message already set in {nameof(ErrorController)}. Existing message:{TempData[TempDataKeys.VacanciesErrorMessage]}. New message:{message}");

            TempData[TempDataKeys.VacanciesErrorMessage] = message;
        }
    }
}