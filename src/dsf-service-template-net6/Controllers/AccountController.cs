using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Data.Models;

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
            //Authentication time
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            if (isOrganization())
            {
                return RedirectToAction("LogOutWithNotAuthorize");

            }
            else if (NotVerified())
            {
                return RedirectToAction("LogOutWithNotAuthorize");
            }
            
            //Set the identidy and Access Token in Session variables
            if (HttpContext.GetTokenAsync("id_token") != null)
            {
                var value = HttpContext.GetTokenAsync("id_token").Result ?? "";
                HttpContext.Session.SetObjectAsJson("id_token",value,authTime);
            }
            if (HttpContext.GetTokenAsync("access_token") != null)
            {
                var value = HttpContext.GetTokenAsync("access_token").Result ?? "";
                HttpContext.Session.SetObjectAsJson("access_token", value, authTime);
            }
            //After CyLogin login, redirect to default home page
            //Check the first session object store once you login
            if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            {
              return RedirectToAction("GetPersonalData", "Citizen", new { currentLanguage = "el", returnUrl = "Address"});
            }
            else
            {
              return  RedirectToAction("LogOut");
            }
        }

        public async Task LogOutWithNotAuthorize()
        {
            HttpContext.Session.Clear();
            var prop = new AuthenticationProperties()
            {
                RedirectUri = "/NoValidProfile"
            };
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, prop);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, prop);

        }
        public async Task LogOut()
        {
            HttpContext.Session.Clear();
            var prop = new AuthenticationProperties()
            {
                RedirectUri = "/"
            };
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, prop);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, prop);
                       
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
