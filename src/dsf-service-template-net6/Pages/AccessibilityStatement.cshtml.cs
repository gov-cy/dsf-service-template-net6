using Dsf.Service.Template.Extensions;
using Dsf.Service.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dsf.Service.Template.Pages
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
