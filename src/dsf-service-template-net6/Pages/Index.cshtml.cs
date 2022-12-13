using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dsf.Service.Template.Pages
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