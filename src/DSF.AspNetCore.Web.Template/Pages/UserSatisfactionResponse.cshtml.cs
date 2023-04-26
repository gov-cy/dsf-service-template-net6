namespace DSF.MOI.CitizenData.Web.Pages;

using DSF.AspNetCore.Web.Template.Extensions;
using DSF.AspNetCore.Web.Template.Pages;
using DSF.AspNetCore.Web.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
       
public class UserSatisfactionResponse : PageModel
{
    private readonly INavigation _navigation;

    public UserSatisfactionResponse(INavigation navigation) 
    {
        _navigation = navigation;
    }

    [FromQuery(Name = "pageSource")]
    [BindProperty(SupportsGet = true)]
    public string PageSource { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetObjectFromJson<bool?>("UserSatisfactionSubmitted") == null)
        {
            return RedirectToPage(nameof(UserSatisfaction));
        }

        HttpContext.Session.Remove("UserSatisfactionSubmitted");

        return Page();
    }
}

