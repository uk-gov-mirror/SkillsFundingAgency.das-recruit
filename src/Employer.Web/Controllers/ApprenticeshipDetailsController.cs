﻿using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.ApprenticeshipDetails;

namespace Employer.Web.Controllers
{
    public class ApprenticeshipDetailsController : Controller
    {
        [HttpGet, Route("apprenticeship-details")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("apprenticeship-details")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "TrainingProvider");
        }
    }
}