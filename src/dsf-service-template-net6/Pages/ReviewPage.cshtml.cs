using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class ReviewPageModel : PageModel
    {
        public CitizenDataResponse _application = new CitizenDataResponse();

        public IActionResult OnGet()
        {
            bool ret = GetCitizenData();
            if (!ret)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }
        private bool GetCitizenData()
        {
            bool isPersonalDataRetrieve = true;
           //First check if user personal data have already being retrieve
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                _application = citizenPersonalDetails;
            } else 
            { 
                //Call Api 
                
            }
            return isPersonalDataRetrieve;
        }
    }
}
