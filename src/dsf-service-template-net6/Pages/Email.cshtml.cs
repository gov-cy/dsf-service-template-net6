using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Extensions;
using Dsf.Service.Template.Services;
using Dsf.Service.Template.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Dsf.Service.Template.Pages
{
    public class EmailModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string CrbEmail { get; set; } = "";
        [BindProperty]
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";

        [BindProperty]
        //Store the email selection Error
        public string EmailSelection { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";
        [BindProperty]
        public string NextLink { get; set; } = "";
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
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_api" || Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#crbEmail'>" + Item.ErrorMessage + "</a>";
                    EmailSelection = Item.ErrorMessage;
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

                EmailSel.email = User.Claims.First(c => c.Type == "email").Value;
            }
            else
            {
                EmailSel.email = res.Data.Email;
            }
        }
        private bool BindData()
        {   //Check if already selected 
            var selectedoptions = _userSession.GetUserEmailData();
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_api)
                {
                    CrbEmail = "1";
                }
                else if (selectedoptions.use_other && (selectedoptions.email == User.Claims.First(c => c.Type == "email").Value || string.IsNullOrEmpty(selectedoptions.email)))
                {
                    //code use when user hit back button on edit page
                    CrbEmail = "1";
                    EmailSel.use_from_api = true;
                    EmailSel.use_other = false;
                    EmailSel.email = string.IsNullOrEmpty(_userSession.GetUserPersonalData()?.Data?.Email) ? User.Claims.First(c => c.Type == "email").Value : _userSession!.GetUserPersonalData()!.Data!.Email;
                    _userSession.SetUserEmailData(EmailSel);
                }
                else
                {
                    CrbEmail = "2";
                }
                return true;
            }
            else
            {
                return false;
            }
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
                    if (res?.Data == null)
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
                EmailSel.use_from_api = true;
                EmailSel.use_other = false;
                EmailSel.email = string.IsNullOrEmpty(_userSession.GetUserPersonalData()?.Data?.Email) ? User.Claims.First(c => c.Type == "email").Value : _userSession!.GetUserPersonalData()!.Data!.Email;

            }
            else if (CrbEmail == "2")
            {
                EmailSel.use_from_api = false;
                EmailSel.use_other = true;
                if (review && !string.IsNullOrEmpty(_userSession.GetUserEmailData()?.email) && _userSession.GetUserEmailData()?.use_from_api == true)
                {
                    //Reset
                    EmailSel.email = "";
                }
                else
                {
                    EmailSel.email = string.IsNullOrEmpty(_userSession.GetUserEmailData()?.email) ? "" : _userSession.GetUserEmailData()!.email;
                }

            }
            else
            {
                EmailSel.use_from_api = false;
                EmailSel.use_other = false;
                EmailSel.email = "";
            }
            if (!review)
            {
                EmailSel.validation_mode = ValidationMode.Select;
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
                EmailSel.validation_mode = ValidationMode.Edit;
            }

            //Model is valid so strore 
            _userSession.SetUserEmailData(EmailSel);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");

            //Set back and Next Link

            if (EmailSel.use_other)
            {
                NextLink = _nav.SetLinks("email-selection", "Email", review, "No");
            }
            else
            {
                NextLink = _nav.SetLinks("email-selection", "Email", review, "Yes");
            }

            if (review)
            {
                return RedirectToPage(NextLink, null, new { review });
            }
            else
            {
                return RedirectToPage(NextLink);
            }
        }
    }
}

