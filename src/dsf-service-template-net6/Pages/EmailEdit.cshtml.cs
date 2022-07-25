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
    [BindProperties]
    public class EmailEditModel : PageModel
    {
        private IValidator<EmailEdit> _validator;
        IStringLocalizer _Loc;
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string EmailErrorClass = "";
        public string EmailSelection = "";
        
        
        public EmailEdit emailEdit { get; set; }
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
            if (HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime) == null)
            {
                ret = false;
            }
            return ret;
        }
        public IActionResult OnGet()
        {
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            //GetData from session 
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
            if (SessionEmailEdit != null)
            {
                emailEdit = SessionEmailEdit;
            }
            //Get Previous mobile number
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                //Defult ariadni value
                emailEdit.email = User.Claims.First(c => c.Type == "email").Value; ;
            }
            return Page();
        }
        public IActionResult OnPostSetEmail(bool review)
        {
            FluentValidation.Results.ValidationResult result = _validator.Validate(emailEdit);
            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState, "emailEdit");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(result);
                return Page();
            }
            //Store Data 
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            HttpContext.Session.Remove("EmailEdit");
            HttpContext.Session.SetObjectAsJson("EmailEdit", emailEdit, authTime);
            //Finally redirect
            return RedirectToPage("/ReviewPage");
        }
    }
}
