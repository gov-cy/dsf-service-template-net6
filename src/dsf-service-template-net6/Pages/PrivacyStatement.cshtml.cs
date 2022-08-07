using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    public class PrivacyStatementModel : BasePage
    {
        public void OnGet()
        {
            SetLinks("PrivacyStatement", false);
        }
    }
}
