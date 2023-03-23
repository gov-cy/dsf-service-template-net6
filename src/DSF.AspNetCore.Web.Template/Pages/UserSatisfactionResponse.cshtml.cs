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

    public IActionResult OnGet()
    {
        // if not coming from UserSatisfaction form
        if ((User.Identity?.IsAuthenticated == false && !Request.Query.ContainsKey("submit"))
            || (User.Identity?.IsAuthenticated == true
            && HttpContext.Session.GetObjectFromJson<bool?>(nameof(UserSatisfaction)) == null))
        {
            return RedirectToPage(nameof(NoPageFoundModel), new { from = "/no-page-found" });
        }
        return Page();
    }

    public IActionResult OnPost(bool review)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage(review ? "/ReviewPage" : _navigation.GetBackLink("/user-satisfaction", review));
        }
        return RedirectToPage("/");
    }
}

