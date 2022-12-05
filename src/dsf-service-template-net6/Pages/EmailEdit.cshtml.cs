using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Pages
{
    
    public class EmailEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IValidator<EmailEdit> _validator;
        [BindProperty]
        //control variables
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string EmailErrorClass { get; set; } = "";

        [BindProperty]
        public string email { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public EmailEdit emailEdit;
        #endregion
        #region "Custom Methods"
        public EmailEditModel(IValidator<EmailEdit> validator, INavigation nav)
        {   _validator = validator;
             emailEdit = new EmailEdit();
            _nav = nav;
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
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
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
            if (GetCitizenDataFromApi == null)
            {
                ret = false;
            }
             return ret;
        }
        private string GetAuthTime()
        {
            return User.Claims.First(c => c.Type == "auth_time").Value;
        }
        private CitizenDataResponse GetCitizenDataFromApi()
        {
            CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", GetAuthTime());
            return res;
        }
        private EmailEdit GetSessionData()
        {
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", GetAuthTime());
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
            if (sessionData != null)
            {
                email = sessionData.email;
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
                email = GetTempSessionData() ;
                ShowErrors(true);
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Update the class before validation
            emailEdit.email = email;
            //Get Previous mobile number
            var citizenPersonalDetails = GetCitizenDataFromApi();
            if (citizenPersonalDetails != null)
            {
                emailEdit.prev_email = citizenPersonalDetails.data.email ??  User.Claims.First(c => c.Type == "email").Value;

            }
            FluentValidation.Results.ValidationResult result = _validator.Validate(emailEdit);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("emailval", email);
                return RedirectToPage("EmailEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Store Data 
            HttpContext.Session.Remove("EmailEdit");
            HttpContext.Session.SetObjectAsJson("EmailEdit", emailEdit, GetAuthTime());
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("emailval");

            //Set back and Next Link
            NextLink = _nav.SetLinks("set-email","Email", review, "NoSelection");
           
                return RedirectToPage(NextLink);
            
        }
    }
   
}
