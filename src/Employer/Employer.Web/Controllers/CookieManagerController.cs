using System.Text.RegularExpressions;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class CookieManagerController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CookieManagerController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("dismiss-outage-message", Name = RouteNames.DismissOutageMessage_Post)]
        public IActionResult DismissOutageMessage([FromForm]string returnUrl)
        {
            const string SeenCookieValue = "1";

            if (!Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage))
                Response.Cookies.Append(CookieNames.SeenOutageMessage, SeenCookieValue, EsfaCookieOptions.GetSingleDayLifetimeHttpCookieOption(_hostingEnvironment));

            if (IsValidReturnUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        private bool IsValidReturnUrl(string returnUrl)
        {
            return Regex.IsMatch(returnUrl, @"^/accounts/[A-Z0-9]{6}/.*");
        }
    }
}