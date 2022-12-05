using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
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
