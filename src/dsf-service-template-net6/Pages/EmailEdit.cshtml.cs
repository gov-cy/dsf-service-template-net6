using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
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
        private IValidator<EmailEdit> _validator;
        IStringLocalizer _Loc;
        [BindProperty]
        //control variables
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string EmailErrorClass { get; set; } = "";
        [BindProperty]
        public string EmailSelection { get; set; } = "";
        [BindProperty]
        public string email { get; set; }
        //Object for session data 
        public EmailEdit emailEdit;
        #endregion
        #region "Custom Methods"
        public EmailEditModel(IValidator<EmailEdit> validator, IStringLocalizer<cEmailEditValidator> Loc)
        {

            _validator = validator;
            _Loc = Loc;
            emailEdit = new EmailEdit();
        }
        void ClearErrors()
        {
            displaySummary = "display:none";
            EmailErrorClass = "";
            ErrorsDesc = "";
        }
        bool ShowErrors()
        {
            if (HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult") != null)

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
            displaySummary = "display:block";
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
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            {
                ret = false;
            }
            if (HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime) == null)
            {
                ret = false;
            }
            if ((HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime) == null) && (HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime) == null))
            {
                ret = false;
            }
            return ret;
        }
        #endregion
        public IActionResult OnGet()
        {
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            //If coming fromPost
            if (!ShowErrors())
            {

                //GetData from session 
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
                if (SessionEmailEdit != null)
                {
                    email = SessionEmailEdit.email;
                }

                //Get Previous mobile number
                var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            }
            else
            {
                email = HttpContext.Session.GetObjectFromJson<string>("emailval");
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Update the class before validation
            emailEdit.email = email;           
            FluentValidation.Results.ValidationResult result = _validator.Validate(emailEdit);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("emailval", email);
                return RedirectToPage("EmailEdit");
            }
            //Store Data 
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            HttpContext.Session.Remove("EmailEdit");
            HttpContext.Session.SetObjectAsJson("EmailEdit", emailEdit, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("emailval");
            //Finally redirect
            return RedirectToPage("/ReviewPage", null, "mainContainer");
        }
    }
   
}
