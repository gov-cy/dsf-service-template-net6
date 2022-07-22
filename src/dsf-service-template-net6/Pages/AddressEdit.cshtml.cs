using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class AddressEditModel : PageModel
    {
        public int selected { get; set; }
        public List<SelectListItem> Staff;
        public AddressEditModel()
        {
            Staff = new List<SelectListItem>
            {
               new SelectListItem {Text = "Shyju", Value = "1"},
               new SelectListItem {Text = "Sean", Value = "2"}
            };
        }

        public void OnGet()
        {

        }
        public void OnPost()
        {
        }
        public void OnPostCheckKeepValue(bool review)
        {
            //Check selected value it should retain the user selection
            //return Page();
        }
        public IActionResult OnPostSetAddress(bool review)
        {
            //Check selected value it should retain the user selection
            return RedirectToPage("/MobileEdit");
        }
    }
}
