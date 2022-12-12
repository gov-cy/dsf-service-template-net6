using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    
    public class EmailEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IUserSession _userSession;
        private readonly IValidator<EmailSection> _validator;
        [BindProperty]
        //control variables
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string EmailErrorClass { get; set; } = "";

        [BindProperty]
        public string Email { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public EmailSection emailEdit;
        #endregion
        #region "Custom Methods"
        public EmailEditModel(IValidator<EmailSection> validator, INavigation nav, IUserSession userSession)
        {
            _validator = validator;
            emailEdit = new EmailSection();
            _nav = nav;
            _userSession = userSession;
        }
        void ClearErrors()
        {
            DisplaySummary = "display:none";
            EmailErrorClass = "";
            ErrorsDesc = "";
        }
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)
            {
                var res = _userSession.GetUserValidationResults()!;
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "emailEdit");
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
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#email'>" + Item.ErrorMessage + "</a>";
                    EmailErrorClass = Item.ErrorMessage;
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
     
      
        private EmailSection GetSessionData()
        {
            var SessionEmailEdit = _userSession.GetUserEmailData();
            return SessionEmailEdit;
        }
        private string GetTempSessionData()
        {
            var tempSession = HttpContext.Session.GetObjectFromJson<string>("emailval");
            return tempSession;
        }
        private bool BindData()
        {   //Check if already selected 
            var sessionData = GetSessionData();
            if (sessionData != null && sessionData?.validation_mode==ValidationMode.Edit)
            {
                Email = sessionData.email;
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
                Email = GetTempSessionData() ;
                ShowErrors(true);
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Update the class before validation
            emailEdit.email = Email;
            emailEdit.use_other = true;
            emailEdit.use_from_api = false;
            emailEdit.validation_mode = ValidationMode.Edit;
            FluentValidation.Results.ValidationResult result = _validator.Validate(emailEdit);
            if (!result.IsValid)
            {
                _userSession.SetUserValidationResults(result);
                HttpContext.Session.SetObjectAsJson("emailval", Email);
                return RedirectToPage("EmailEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Store Data 
            _userSession.SetUserEmailData(emailEdit);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("emailval");
         
            //Set back and Next Link
           NextLink = _nav.SetLinks("set-email","Email", review, "NoSelection");
           
                return RedirectToPage(NextLink);
            
        }
    }
   
}
