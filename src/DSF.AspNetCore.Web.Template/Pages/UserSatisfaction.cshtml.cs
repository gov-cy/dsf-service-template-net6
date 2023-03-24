namespace DSF.AspNetCore.Web.Template.Pages;

using DSF.AspNetCore.Web.Template.Extensions;
using DSF.AspNetCore.Web.Template.Services;
using DSF.AspNetCore.Web.Template.Services.Model;
using DSF.AspNetCore.Web.Template.Services.UserSatisfaction;
using DSF.AspNetCore.Web.Template.Services.UserSatisfaction.Data;
using DSF.Localization;
using DSF.MOI.CitizenData.Web.Pages;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class UserSatisfaction : PageModel
{
    [BindProperty]
    public string DisplaySummary { get; set; } = "display:none";
    [BindProperty]
    public string ErrorsDesc { get; set; } = string.Empty;
    [BindProperty]
    public string BackLink { get; set; } = string.Empty;

    [BindProperty]
    //Store the email selection Error
    public string SatisfactionSelection { get; set; } = string.Empty;

    [BindProperty]
    //Store the email selection Error
    public string? HowCouldWeImprove { get; set; } = string.Empty;

    private readonly IUserSatisfactionService _userSatisfactionService;
    private readonly IResourceViewLocalizer _localizer;
    private readonly INavigation _navigation;
    private readonly IUserSession _userSession;
    private readonly IValidator<UserSatisfaction> _validator;

    public UserSatisfaction(INavigation navigation, IUserSession userSession, IUserSatisfactionService userSatisfactionService, IResourceViewLocalizer localizer, IValidator<UserSatisfaction> validator)
    {
        _navigation = navigation;
        _userSession = userSession;
        _localizer = localizer;
        _userSatisfactionService = userSatisfactionService;
        _validator = validator;
    }

    public IActionResult OnGet(bool review, bool fromPost)
    {
        BackLink = _navigation.GetBackLink("/user-satisfaction", review);
        ShowErrors(fromPost);
     
        return Page();
    }

    public IActionResult OnPost(bool review)
    {
        var result = _validator.Validate(this);
        // valid form post and user has not submitted in same session
        if (!result.IsValid)
        {
            _userSession.SetUserValidationResults(result);
            return RedirectToPage(nameof(UserSatisfaction), null, new { fromPost = true }, "mainContainer");
        }

        if (HttpContext.Session.GetObjectFromJson<bool?>(nameof(UserSatisfaction)) == false)
        {
            result.Errors.Add(new ValidationFailure()
            {
                ErrorMessage = _localizer["user-satisfaction-response.already_submit"]
            });
            return RedirectToPage(nameof(UserSatisfaction), null, new { fromPost = true }, "mainContainer");
        }

        object? routeValues = new { submit = true };
        string sessionId = string.Empty;
        if (User.Identity?.IsAuthenticated == true)
        {
            HttpContext.Session.SetObjectAsJson(nameof(UserSatisfaction), true);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            routeValues = null;
            sessionId = HttpContext.Session.Id;
        }

        _userSatisfactionService.SubmitUserSatisfaction(new UserSatisfactionServiceRequest()
        {
            PageSource = Request.Path.Value!,
            Rating = Enumeration.FromName<Feedback>(SatisfactionSelection).Id,
            Description = HowCouldWeImprove ?? string.Empty,
            AccessToken = _userSession.GetAccessToken() ?? string.Empty,
            SessionId = sessionId
        });

        return RedirectToPage(nameof(UserSatisfactionResponse), routeValues);
    }
    private void AddError(string propertyName, string errorMessage)
    {
        ModelState.AddModelError(propertyName, _localizer["ServerError_Title"]);
        HttpContext.Session.SetObjectAsJson($"valresult", new ValidationResult()
        {
            Errors = new List<ValidationFailure>()
            {
                new ValidationFailure()
                {
                    ErrorMessage = errorMessage,
                    PropertyName = propertyName
                }
            }
        });
    }

    bool ShowErrors(bool fromPost)
    {
        if (fromPost)
        {
            var res = _userSession.GetUserValidationResults()!;
            // Copy the validation results into ModelState.
            // ASP.NET uses the ModelState collection to populate 
            // error messages in the View.
            res.AddToModelState(this.ModelState, "SatisfactionSelection");
            //Update Error messages on View
            ClearErrors();
            SetViewErrorMessages(res);
            return true;
        }
        else
        {
            return false;
        }
    }

    void ClearErrors()
    {
        DisplaySummary = "display:none";
        SatisfactionSelection = "";
        ErrorsDesc = "";
    }

    private void SetViewErrorMessages(ValidationResult result)
    {
        if (result != null)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == nameof(SatisfactionSelection))
                {
                    ErrorsDesc += "<a href='#SatisfactionSelection'>" + Item.ErrorMessage + "</a>";
                    SatisfactionSelection = Item.ErrorMessage;
                }
            }
        }

    }

    public class Feedback : Enumeration
    {
        public static readonly Feedback VerySatisfactory = new(5, nameof(VerySatisfactory));
        public static readonly Feedback Satisfactory = new(4, nameof(Satisfactory));
        public static readonly Feedback NeitherNor = new(3, nameof(NeitherNor));
        public static readonly Feedback Dissatisfactory = new(2, nameof(Dissatisfactory));
        public static readonly Feedback VeryDissatisfactory = new(1, nameof(VeryDissatisfactory));

        public Feedback(int id, string name) : base(id, name) { }
    }
}
