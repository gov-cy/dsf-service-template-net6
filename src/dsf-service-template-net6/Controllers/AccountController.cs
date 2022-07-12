using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace dsf_service_template_net6.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        private bool isOrganization()
        {
            var cp = (ClaimsPrincipal)User;
            var id = cp.Claims.FirstOrDefault(c => c.Type == "legal_unique_identifier")?.Value;
            if (!string.IsNullOrEmpty(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool NotVerified()
        {
            var cp = (ClaimsPrincipal)User;
            var id = cp.Claims.FirstOrDefault(c => c.Type == "unique_identifier")?.Value;
            if (string.IsNullOrEmpty(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        //The Authorize Tag will redirect to CY Login for the user to login
        [Authorize]
        public IActionResult LogIn()
        {
            if (isOrganization())
            {
                return Redirect("/NoValidProfile");

            }
            else if (NotVerified())
            {
                return Redirect("/NoValidProfile");
            }
            //After CyLogin login, redirect to default home page
            return Redirect("/");
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();            

            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme);
        }

        //Ariadni requirement so that they can logout the user with a get call to the account countroller
        public void OidcSignOut(string sid)
        {
            var cp = (ClaimsPrincipal)User;
            var sidClaim = cp.FindFirst("sid");
            if (sidClaim?.Value == sid)
            {
                // logout
                SignOut(CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme);
            }
        }
    }
}
