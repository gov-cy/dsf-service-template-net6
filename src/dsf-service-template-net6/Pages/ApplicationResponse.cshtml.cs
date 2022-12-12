using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
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
