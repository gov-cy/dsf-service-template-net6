using DSF.AspNetCore.Web.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DSF.AspNetCore.Web.Template.Pages
{
    public class CookiePolicyModel : PageModel
    {
        [BindProperty]
        public string BackLink { get; set; } = "";
        private readonly INavigation _nav;
        public CookiePolicyModel(INavigation nav)
        {
            _nav = nav;
        }
        public void OnGet()
        {
            BackLink = _nav.GetBackLink("/cookie-policy", false);
        }
      
    }
}
