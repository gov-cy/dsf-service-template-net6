using DSF.AspNetCore.Web.Template.Services;
using DSF.AspNetCore.Web.Template.Services.Model;
using DSF.AspNetCore.Web.Template.Data.Models;
using DSF.AspNetCore.Web.Template.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http.Extensions;

namespace DSF.AspNetCore.Web.Template.Pages
{
    public class EmailModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string CrbEmail { get; set; } = string.Empty;
        [BindProperty]
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = string.Empty;

        [BindProperty]
        //Store the email selection Error
        public string EmailSelection { get; set; } = string.Empty;
        [BindProperty]
        public string BackLink { get; set; } = string.Empty;
        [BindProperty]
        public string NextLink { get; set; } = string.Empty;

        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IContact _service;
        private readonly IUserSession _userSession;
        private readonly IValidator<EmailSection> _validator;
        //Object for session data 
        public EmailSection EmailSel;
        #endregion

        #region "Custom Methods"
        public EmailModel(IValidator<EmailSection> validator, IContact service, INavigation nav, IUserSession userSession)
        {
            _service = service;
            _validator = validator;
            _nav = nav;
            _userSession = userSession;
            EmailSel = new EmailSection();
        }
        //Use to show error messages if web form has errors
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)
            {
                var res = _userSession.GetUserValidationResults()!;
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "Email_select");
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
            EmailSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            if (result != null)
            {
                //First Enable Summary Display
                DisplaySummary = "display:block";
                //Then Build Summary Error
                foreach (ValidationFailure Item in result.Errors)
                {
                    if (Item.PropertyName == nameof(EmailSel.UseFromApi) || Item.PropertyName == nameof(EmailSel.Email))
                    {
                        ErrorsDesc += "<a href='#crbEmail'>" + Item.ErrorMessage + "</a>";
                        EmailSelection = Item.ErrorMessage;
                    }

                }
            }

        }
        private bool AllowToProceed()
        {
            bool ret = true;
            //For the demo purpose we might not get data from template api service
            //if (_userSession.GetUserPersonalData() == null)
            //{
            //    ret = false;
            //}
            return ret;
        }

        private void BindSelectionData()
        {
            ContactInfoResponse res = _userSession.GetUserPersonalData()!;
            //Set Email info to model class
            if (string.IsNullOrEmpty(res?.Data?.Email))
            {

                EmailSel.Email = User.Claims.First(c => c.Type == "email").Value;
            }
            else
            {
                EmailSel.Email = res.Data.Email;
            }
        }

        private bool BindData()
        {   //Check if already selected 
            var selectedoptions = _userSession.GetUserEmailData();
            if (selectedoptions != null)
            {
                if (selectedoptions.UseFromApi)
                {
                    CrbEmail = "1";
                }
                else if (selectedoptions.UseOther && (selectedoptions.Email == _userSession?.GetUserPersonalData()?.Data?.Email || string.IsNullOrEmpty(selectedoptions.Email)))
                {
                    //code use when user hit back button on edit page
                    CrbEmail = "1";
                    EmailSel.UseFromApi = true;
                    EmailSel.UseOther = false;
                    EmailSel.Email = string.IsNullOrEmpty(_userSession.GetUserPersonalData()?.Data?.Email) ? User.Claims.First(c => c.Type == "email").Value : _userSession!.GetUserPersonalData()!.Data!.Email;
                    _userSession.SetUserEmailData(EmailSel);
                }
                else
                {
                    CrbEmail = "2";
                }
                return true;
            }
            return false;
        }
        #endregion

        public IActionResult OnGet(bool review, bool fromPost)
        {

            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            // First set back link
            BackLink = _nav.GetBackLink("/email-selection", review);
            //Show selection Data 
            BindSelectionData();
            if (fromPost)
            {
                ShowErrors(true);
            }
            else
            {
                //check for revisit
                bool revisit = BindData();
                if (!revisit)
                {
                    //Check whether api data were retrieve from login , otherwise call again
                    ContactInfoResponse res = _userSession.GetUserPersonalData() ?? new ContactInfoResponse();
                    if (string.IsNullOrEmpty(res?.Data?.Email))
                    {
                        //Cet the citizen personal details from civil registry
                        res = _service.GetContact(_userSession.GetAccessToken()!);
                        //Demo handling
                        if (res.Succeeded)
                        {
                            _userSession.SetUserPersonalData(res);
                        }
                        //Real example handling
                        //if (res.succeeded == false)
                        //{
                        //    return RedirectToPage("/ServerError");
                        //}
                        //else
                        //{
                        //    //if the user is already login and not passed from login, set in session
                        //    _userSession.SetUserPersonalData(res);
                        //}
                    }
                }
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            if (CrbEmail == "1")
            {
                EmailSel.UseFromApi = true;
                EmailSel.UseOther = false;
                EmailSel.Email = string.IsNullOrEmpty(_userSession.GetUserPersonalData()?.Data?.Email)
                    ? User.Claims.First(c => c.Type == "email").Value
                    : _userSession!.GetUserPersonalData()!.Data!.Email;
            }
            else if (CrbEmail == "2")
            {
                EmailSel.UseFromApi = false;
                EmailSel.UseOther = true;
                if (review && !string.IsNullOrEmpty(_userSession.GetUserEmailData()?.Email) && _userSession.GetUserEmailData()?.UseFromApi == true)
                {
                    //Reset
                    EmailSel.Email = "";
                }
                else
                {
                    EmailSel.Email = string.IsNullOrEmpty(_userSession.GetUserEmailData()?.Email) ? "" : _userSession.GetUserEmailData()!.Email;
                }
            }
            else
            {
                EmailSel.UseFromApi = false;
                EmailSel.UseOther = false;
                EmailSel.Email = string.Empty;
            }

            if (!review && _userSession.GetUserEmailData() == null)
            {
                EmailSel.ValidationMode = ValidationMode.Select;
                //Validate Model
                FluentValidation.Results.ValidationResult result = _validator.Validate(EmailSel);
                if (!result.IsValid)
                {
                    _userSession.SetUserValidationResults(result);
                    return RedirectToPage("Email", null, new { fromPost = true }, "mainContainer");
                }
            }
            else
            {
                EmailSel.ValidationMode = ValidationMode.Edit;
            }

            //Model is valid so strore 
            _userSession.SetUserEmailData(EmailSel);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");

            //Set back and Next Link

            if (EmailSel.UseOther)
            {
                NextLink = _nav.SetLinks("email-selection", "Email", review, "No");
            }
            else
            {
                NextLink = _nav.SetLinks("email-selection", "Email", review, "Yes");
            }
            //clear # in  URLs  that generates on error found
            bool hashInUrl = HttpContext.Request.GetDisplayUrl().Contains("fromPost");
            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review.ToString().ToLower() }, !hashInUrl ? null : "");
            }
            else
            {
                return RedirectToPage(NextLink, null, null, !hashInUrl ? null : "");
            }
        }
    }


}

