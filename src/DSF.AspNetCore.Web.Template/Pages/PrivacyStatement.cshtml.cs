using DSF.AspNetCore.Web.Template.Extensions;
using DSF.AspNetCore.Web.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DSF.AspNetCore.Web.Pages
{
    public class PrivacyStatementModel : PageModel
    {
        [BindProperty]
        public string BackLink { get; set; } = "";
        private readonly INavigation _nav;
        public PrivacyStatementModel(INavigation nav)
        {
            _nav = nav;
        }
        public void OnGet()
        {
            BackLink = _nav.GetBackLink("/privacy-statement", false);
        }

    }
}
