using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    public class IndexModel : PageModel
    {      
        public IActionResult OnPostApplicationStart()
        {
            //it will redirect to first wizard page
            return RedirectToPage("/Email");

        }
    }
}