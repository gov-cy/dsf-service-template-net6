using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DSF.AspNetCore.Web.Template.Pages
{
    public class NoValidProfileModel : PageModel
    {
        public void OnGet()
        {
            HttpContext.Session.Clear();
        }
    }
}
