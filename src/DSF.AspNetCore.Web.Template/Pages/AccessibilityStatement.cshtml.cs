using DSF.AspNetCore.Web.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DSF.AspNetCore.Web.Pages
{
    public class AccessibilityStatementModel : PageModel
    {
        [BindProperty]
        public string BackLink { get; set; } = "";
        private readonly INavigation _nav;
        public AccessibilityStatementModel(INavigation nav)
        {
            _nav = nav;
        }
        public void OnGet()
        {
            BackLink = _nav.GetBackLink("/accessibility-statement", false);   
        }
      
    }
}
