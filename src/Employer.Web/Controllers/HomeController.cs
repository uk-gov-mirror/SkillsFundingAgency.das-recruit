﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Employer.Domain.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IOptions<ExternalLinksConfiguration> externalLinksOptions, ILogger<HomeController> logger)
        {
            _externalLinks = externalLinksOptions.Value;
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            _logger.LogInformation("Showing Index page.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            return Redirect(_externalLinks.ManageApprenticeshipSiteUrl);
        }
    }
}
