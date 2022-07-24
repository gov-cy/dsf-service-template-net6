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
    public class MobileModel : PageModel
    {
        #region "Variables"
        //control variables
        public string crbMobile { get; set; }
        public string option1 { get; set; } = "false";
        public string option2 { get; set; } = "false";
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string MobileSelection = "";
        //Dependancy injection Variables
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<MobileSelect> _validator;
        //Object for session data 
        public MobileSelect Mobile_select;
        #endregion
        #region "Custom Methods"
        public MobileModel(IValidator<MobileSelect> validator, IMyHttpClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
            _validator = validator;
            Mobile_select = new MobileSelect();
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
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "mobile")
                {
                    ErrorsDesc += "<a href='#crbMobile'>" + Item.ErrorMessage + "</a>";
                    MobileSelection = Item.ErrorMessage;
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
            return ret;
        }
        #endregion
        public void OnGet()
        {
            //Chack if user has sequentialy load the page
            bool allow=AllowToProceed();
            if (!allow)
            {
               RedirectToAction("LogOut", "Account");
            }
            //Get  Citize details loaded
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res= HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
           
            //Set Mobile info to model class
            Mobile_select.mobile = res.data.mobile;
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime);
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
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (crbMobile == "1")
            {
                Mobile_select.use_from_civil = true;
                Mobile_select.use_other = false;
            }
            else if (crbMobile == "2")
            {
                Mobile_select.use_from_civil = false;
                Mobile_select.use_other = true;
            }
            else
            {
                Mobile_select.use_from_civil = false;
                Mobile_select.use_other = false;
            }
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            Mobile_select.mobile = citizen_data.data.mobile;
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Mobile_select);
            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState, "Mobile_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(result);
                return Page();
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("MobileSelect");
            HttpContext.Session.SetObjectAsJson("MobileSelect", Mobile_select, authTime);
            //Finally redirect
            if (review)
            {
                if (Mobile_select.use_other)
                {
                    return RedirectToPage("/MobileEdit", new { review = "true" });
                }
                else
                {
                    return RedirectToPage("/ReviewPage");
                }
            }
            else
            {
                if (Mobile_select.use_other)
                {
                    return RedirectToPage("/MobileEdit");
                }
                else
                {
                    return RedirectToPage("/EmailEdit");
                }
            }

        }
    }
}
