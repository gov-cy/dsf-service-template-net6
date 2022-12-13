using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Extensions;
using Dsf.Service.Template.Services;
using Dsf.Service.Template.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Dsf.Service.Template.Pages
{
    [BindProperties]
    public class MobileModel : PageModel
    {
        #region "Variables"
        //control variables
        public string CrbMobile { get; set; } = "";
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string MobileSelection = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";

        //Dependancy injection Variables
        private readonly IUserSession _userSession;
        private readonly INavigation _nav;
        private readonly IValidator<MobileSection> _validator;
        //Object for session data 
        public MobileSection MobileSel;
        #endregion
        #region "Custom Methods"
        public MobileModel(IValidator<MobileSection> validator, INavigation nav, IUserSession userSession)
        {
            _nav = nav;
            _validator = validator;
            MobileSel = new MobileSection();
            _userSession = userSession;
        }
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

            {
                var res = _userSession.GetUserValidationResults()!;
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "Mobile_select");
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
            displaySummary = "display:none";
            MobileSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_api" || Item.PropertyName == "mobile")
                {
                    ErrorsDesc += "<a href='#crbMobile'>" + Item.ErrorMessage + "</a>";
                    MobileSelection = Item.ErrorMessage;
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
            if (_userSession.GetUserEmailData() == null)
            {
                ret = false;
            }
            return ret;
        }



        private MobileSection GetSessionData()
        {
            var selectedoptions = _userSession.GetUserMobileData();
            return selectedoptions;
        }
        private void BindSelectionData()
        {
            ContactInfoResponse? res = _userSession.GetUserPersonalData();
            //Set Email info to model class
            if (!string.IsNullOrEmpty(res?.Data?.MobileTelephone))
            {

                MobileSel.mobile = res.Data.MobileTelephone;
            }

        }
        private bool BindData()
        {   //Check if already selected 
            var selectedoptions = GetSessionData();
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_api)
                {
                    CrbMobile = "1";
                }
                else if (selectedoptions.use_other && (selectedoptions?.mobile == _userSession?.GetUserPersonalData()?.Data?.MobileTelephone ||string.IsNullOrEmpty(selectedoptions?.mobile)) )
                {
                    //code use when user hit back button on edit page
                    CrbMobile = "1";
                    MobileSel.use_from_api = true;
                    MobileSel.use_other = false;
                    MobileSel.mobile = _userSession?.GetUserPersonalData()?.Data?.MobileTelephone ?? "";
                    _userSession!.SetUserMobileData(MobileSel);

                }
                else
                {
                    CrbMobile = "2";
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
            BackLink = _nav.GetBackLink("/mobile-selection", review);
            //Show selection Data 
            BindSelectionData();
            if (fromPost)
            {
                ShowErrors(true);
            }
            else
            {
                BindData();
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (CrbMobile == "1")
            {
                MobileSel.use_from_api = true;
                MobileSel.use_other = false;
                MobileSel.mobile = _userSession!.GetUserPersonalData()!.Data!.MobileTelephone;
            }
            else if (CrbMobile == "2")
            {
                MobileSel.use_from_api = false;
                MobileSel.use_other = true;
                if (review && !string.IsNullOrEmpty(_userSession.GetUserMobileData()?.mobile) && _userSession.GetUserMobileData()?.use_from_api == true)
                {
                    //Reset
                    MobileSel.mobile = "";
                }
                else
                {
                    MobileSel.mobile = string.IsNullOrEmpty(_userSession.GetUserMobileData()?.mobile) ? "" : _userSession.GetUserMobileData()!.mobile;
                }
               
            }
            else
            {
                MobileSel.use_from_api = false;
                MobileSel.use_other = false;
            }
            if (!review)
            {
                
                MobileSel.validation_mode = ValidationMode.Select;
                //Validate Model
                FluentValidation.Results.ValidationResult result = _validator.Validate(MobileSel);
                if (!result.IsValid)
                {
                    _userSession.SetUserValidationResults(result);
                    return RedirectToPage("Mobile", null, new { fromPost = true }, "mainContainer");
                }
            }
            else
            {
                MobileSel.validation_mode = ValidationMode.Edit;
            }


            //Model is valid so strore 
            _userSession.SetUserMobileData(MobileSel);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect

            //Set back and Next Link

            if (MobileSel.use_other)
            {
                NextLink = _nav.SetLinks("mobile-selection", "Mobile", review, "No");
            }
            else
            {
                NextLink = _nav.SetLinks("mobile-selection", "Mobile", review, "Yes");
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
