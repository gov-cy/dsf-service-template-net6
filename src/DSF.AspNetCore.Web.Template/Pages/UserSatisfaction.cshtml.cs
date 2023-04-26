namespace DSF.AspNetCore.Web.Template.Pages;

using DSF.AspNetCore.Web.Template.Data.Models;
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
    public string SatisfactionSelection { get; set; } = string.Empty;

    [BindProperty]
    public string? HowCouldWeImprove { get; set; } = string.Empty;

    [FromQuery(Name = "pageSource")]
    public string PageSource { get; set; } = string.Empty;

    private readonly IUserSatisfactionService _userSatisfactionService;
    private readonly IResourceViewLocalizer _localizer;
    private readonly INavigation _navigation;
    private readonly IUserSession _userSession;
    private readonly IValidator<UserSatisfactionViewModel> _validator;

    public UserSatisfaction(INavigation navigation, IUserSession userSession, IUserSatisfactionService userSatisfactionService, IResourceViewLocalizer localizer, IValidator<UserSatisfactionViewModel> validator)
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

        bool showErrors = fromPost;
        if (!fromPost && HttpContext.Session.GetObjectFromJson<bool?>("UserSatisfactionAlreadySubmitted") == true)
        {
            var valresult = new ValidationResult();
            valresult.Errors.Add(new ValidationFailure()
            {
                ErrorMessage = _localizer["user-satisfaction.Custom.exists"],
                PropertyName = null
            });
            HttpContext.Session.SetObjectAsJson("valresult", valresult);
            showErrors = true;
        }

        // load previously set form values
        var userSatisfactionSession = HttpContext.Session.GetObjectFromJson<UserSatisfactionViewModel>(nameof(UserSatisfactionViewModel));
        HowCouldWeImprove = userSatisfactionSession?.HowCouldWeImprove!;
        SatisfactionSelection = userSatisfactionSession?.SatisfactionSelection!;

        ShowErrors(showErrors);
     
        return Page();
    }

    public IActionResult OnPost(bool review)
    {
        if (HttpContext.Session.GetObjectFromJson<bool?>("UserSatisfactionAlreadySubmitted") == true)
        {
            return RedirectToPage(nameof(UserSatisfaction), null, "mainContainer");
        }
        var viewModel = new UserSatisfactionViewModel
        {
            HowCouldWeImprove = this.HowCouldWeImprove ?? string.Empty,
            SatisfactionSelection = this.SatisfactionSelection
        };

        var result = _validator.Validate(viewModel);

        // valid form post and user has not submitted in same session
        if (!result.IsValid)
        {
            HttpContext.Session.SetObjectAsJson(nameof(UserSatisfactionViewModel), viewModel);
            _userSession.SetUserValidationResults(result);
            return RedirectToPage(nameof(UserSatisfaction), null, new { fromPost = true, pageSource = GetPageSource() }, "mainContainer");
        }

        //Remove Error Session 
        HttpContext.Session.Remove("valresult");
        HttpContext.Session.Remove(nameof(UserSatisfactionViewModel));

        var response = _userSatisfactionService.SubmitUserSatisfaction(new UserSatisfactionServiceRequest()
        {
            PageSource = Request.Path.Value!,
            Rating = Enumeration.FromName<Feedback>(SatisfactionSelection).Id,
            Description = HowCouldWeImprove ?? string.Empty
        });

        HttpContext.Session.SetObjectAsJson("UserSatisfactionAlreadySubmitted", true);
        HttpContext.Session.SetObjectAsJson("UserSatisfactionSubmitted", true);

        return RedirectToPage(nameof(UserSatisfactionResponse), new { submit = true, pageSource = GetPageSource() });
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
            res.AddToModelState(this.ModelState);
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
        HttpContext.Session.Remove("valresult");
    }

    private void SetViewErrorMessages(ValidationResult result)
    {
        if (result != null)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure item in result.Errors)
            {
                ErrorsDesc += $"<a href='#{item.PropertyName}'>" + item.ErrorMessage + "</a>";   
            }
        }

    }

    private string GetPageSource()
    {
        if (string.IsNullOrEmpty(PageSource))
        {
            PageSource = Request.Path.Value?.IndexOf("/", 1) > 0
                ? Request.Path.Value[..Request.Path.Value!.IndexOf("/", 1)]
                : Request.Path.Value!;
        }
        return PageSource;
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
