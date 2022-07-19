using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class MobileEditModel : PageModel
    {
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public void OnGet()
        {
        }
    }
}
