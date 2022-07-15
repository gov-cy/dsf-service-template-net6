using dsf_service_template_net6.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class ReviewPageModel : PageModel
    {
        public CitizenDataResponse _application = new CitizenDataResponse();

        public void OnGet()
        {
        }
    }
}
