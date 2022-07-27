using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class EmailModel : PageModel
    {
        #region "Variables"
        //control variables
        public string crbEmail { get; set; }
        public string option1 { get; set; } = "false";
        public string option2 { get; set; } = "false";
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string EmailSelection = "";
        //Dependancy injection Variables
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<EmailSelect> _validator;
        //Object for session data 
        public EmailSelect Email_select;
        #endregion
        #region "Custom Methods"
        public EmailModel(IValidator<EmailSelect> validator, IMyHttpClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
            _validator = validator;
            Email_select = new EmailSelect();
        }
        void ClearErrors()
        {
            displaySummary = "display:none";
            EmailSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#crbEmail'>" + Item.ErrorMessage + "</a>";
                    EmailSelection = Item.ErrorMessage;
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
              return  RedirectToAction("LogOut", "Account");
            }
            //Get  Citize details loaded
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);

            //Set Email info to model class
            Email_select.email = res.data.email;
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", authTime);
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_civil)
                {
                    option1 = "true";
                    option2 = "false";
                }
                else
                {
                    option1 = "false";
                    option2 = "true";
                }
            }
            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (crbEmail == "1")
            {
                Email_select.use_from_civil = true;
                Email_select.use_other = false;
            }
            else if (crbEmail == "2")
            {
                Email_select.use_from_civil = false;
                Email_select.use_other = true;
            }
            else
            {
                Email_select.use_from_civil = false;
                Email_select.use_other = false;
            }
            //Re-assign defult email
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            Email_select.email = citizen_data.data.email;
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Email_select);
            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState, "Email_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(result);
                return Page();
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("EmailSelect");
            HttpContext.Session.SetObjectAsJson("EmailSelect", Email_select, authTime);
            //Finally redirect
            if (review)
            {
                if (Email_select.use_other)
                {
                    return RedirectToPage("/EmailEdit", new { review = "true" });
                }
                else
                {
                    return RedirectToPage("/ReviewPage", null, "RedirectTarget");
                }
            }
            else
            {
                if (Email_select.use_other)
                {
                    return RedirectToPage("/EmailEdit", null, "RedirectTarget");
                }
                else
                {
                    return RedirectToPage("/ReviewPage", null, "RedirectTarget");
                }
            }

        }
    }
}

