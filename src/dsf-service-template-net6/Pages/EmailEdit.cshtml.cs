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
                if (Item.PropertyName == "otherEmail")
                {
                    ErrorsDesc += "<a href='#otherEmail'>" + Item.ErrorMessage + "</a>";
                    EmailErrorClass = Item.ErrorMessage;
                }
                if (Item.PropertyName == "useAriadni")
                {
                    ErrorsDesc += "<a href='#useAriadni'>" + Item.ErrorMessage + "</a>";
                    EmailSelection = Item.ErrorMessage;
                }
            }
        }
        public void OnGet()
        {
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
            if (SessionEmailEdit != null)
            {
                emailEdit = SessionEmailEdit;
            }
            //Get Previous mobile number
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                emailEdit.otherEmail = citizenPersonalDetails.data.email;
            }
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
            //Mob Edit from Session
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
            if (SessionEmailEdit != null)
            {
                SessionEmailEdit.otherEmail = emailEdit.otherEmail;
                
                HttpContext.Session.Remove("EmailEdit");
                HttpContext.Session.SetObjectAsJson("EmailEdit", SessionEmailEdit, authTime);
            }
            else
            {
                HttpContext.Session.SetObjectAsJson("EmailEdit", emailEdit, authTime);
            }
            //Generate One time password

            //Finally redirect
            return RedirectToPage("/ReviewPage");
        }
    }
}
