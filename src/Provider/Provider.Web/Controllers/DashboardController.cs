using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(User.GetUkprn());
            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.DashboardInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.DashboardInfoMessage].ToString();

            return View(vm);
        }
    }
}