using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class ApplicationResponseModel : PageModel
    {
       public string _applResponse=String.Empty;
        
        public void OnGet()
        {
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            _applResponse = HttpContext.Session.GetObjectFromJson<string>("ref_no", authTime);
            HttpContext.Session.Clear();
            //Keep the refence in session for lang change
            HttpContext.Session.SetObjectAsJson("ref_no", _applResponse, authTime);
        }
    }
}
