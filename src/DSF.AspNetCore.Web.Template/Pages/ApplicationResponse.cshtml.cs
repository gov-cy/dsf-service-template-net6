using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Extensions;
using Dsf.Service.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dsf.Service.Template.Pages
{
    [BindProperties]
    public class ApplicationResponseModel : PageModel
    {
       public string _applResponse=String.Empty;
        private readonly IUserSession _userSession;
        public ApplicationResponseModel(IUserSession userSession)
        {            
            _userSession = userSession;
        }

        public void OnGet()
        {            
            _applResponse = _userSession.GetUserReferenceNumber();
            HttpContext.Session.Clear();
            //Keep the refence in session for lang change
            _userSession.SetUserReferenceNumber(_applResponse);
        }
    }
}
