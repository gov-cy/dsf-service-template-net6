using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Services.Model;
using dsf_service_template_net6.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using dsf_service_template_net6.Services;

namespace dsf_service_template_net6.Pages
{
       public class MobileEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IUserSession _userSession;
        private readonly IValidator<MobileSection> _validator;
         //control variables
        [BindProperty]
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string MobileErrorClass { get; set; } = "";
        [BindProperty]
        public string mobile { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public MobileSection mobEdit { get; set; }
       
        #endregion
        #region "Custom Methods"
        public MobileEditModel(IValidator<MobileSection> validator, INavigation nav, IUserSession userSession)
        {  _validator = validator;
            _userSession= userSession;
            mobEdit = new MobileSection();
            _nav = nav;
        }
     
        void ClearErrors()
        {
            displaySummary = "display:none";
            MobileErrorClass = "";
            ErrorsDesc = "";
        }
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

            {
                var res = _userSession.GetUserValidationResults();
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "mobEdit");
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
        private void SetViewErrorMessages(ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "mobile")
                {
                    ErrorsDesc += "<a href='#mobile'>" + Item.ErrorMessage + "</a>";
                    MobileErrorClass = Item.ErrorMessage;
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
            var SessionEmailEdit = _userSession.GetUserMobileData();
            return SessionEmailEdit;
        }
        private string GetTempSessionData()
        {
            var tempSession = HttpContext.Session.GetObjectFromJson<string>("mobileval");
            return tempSession;
        }
        private bool BindData()
        {   //Check if already selected 
            var sessionData = GetSessionData();
            if (sessionData != null)
            {
                mobile = sessionData.mobile;
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
            //Set Back Link
            BackLink = _nav.GetBackLink("/set-email", review);
            //If coming fromPost
            if (!fromPost)
            { //GetData from session 
                BindData();
            }
            else
            {
                mobile = GetTempSessionData();
                ShowErrors(true);
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        { // Update the class before validation
            mobEdit.mobile = mobile;
            //Get Previous mobile number
            mobEdit.validation_mode = ValidationMode.Edit;           
            FluentValidation.Results.ValidationResult result = _validator.Validate(mobEdit);
            if (!result.IsValid)
            {
                _userSession.SetUserValidationResults(result);
                HttpContext.Session.SetObjectAsJson("mobileval", mobile);
                return RedirectToPage("MobileEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Mob Edit from Session
                _userSession.SetUserMobileData(mobEdit);

            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("mobileval");
    
            //Set back and Next Link
            NextLink=_nav.SetLinks("set-mobile", "Mobile", review, "NoSelection");
            return RedirectToPage(NextLink);
        }
    }
}
