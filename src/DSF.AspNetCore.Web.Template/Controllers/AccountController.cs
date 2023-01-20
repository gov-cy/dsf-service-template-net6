using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DSF.AspNetCore.Web.Template.Services;

namespace DSF.AspNetCore.Web.Template.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IUserSession _userSession;

        public AccountController(IUserSession userSession)
        {
          _userSession = userSession;
        }
        private bool IsOrganization()
        {
            var cp = User;
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
            var cp = User;
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
            if (IsOrganization())
            {
                return RedirectToAction("LogOutWithNotAuthorize");

            }
            else if (NotVerified())
            {
                return RedirectToAction("LogOutWithNotAuthorize");
            }
            
            //Set Access Token in Session variables
           
            if (_userSession.GetAccessToken() != null)
            {
                var value = _userSession.GetAccessToken() ?? "";
                _userSession.SetAccessToken(value);
            }
            //After CyLogin login, redirect to the first page of the flow
            return RedirectToPage("/Email");
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
            var cp = User;
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
