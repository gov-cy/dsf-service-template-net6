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
    public class AddressModel : PageModel
    {
        #region "Variables"
        //control variables
        public string crbAddress { get; set; }
        public string option1 { get; set; } = "false";
        public string option2 { get; set; } = "false";
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string AddressSelection = "";
        //Dependancy injection Variables
        private readonly ILogger<AddressModel> _logger;
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<AddressSelect> _validator;
        //Object for session data 
        public AddressSelect address_select;
        #endregion
        #region "Custom Methods"
         public AddressModel(IValidator<AddressSelect> validator, IMyHttpClient client, IConfiguration config, ILogger<AddressModel> logger)
        {
            _client = client;
            _configuration = config;
            _validator = validator;
            address_select = new AddressSelect();
            _logger = logger;
        }
        void ClearErrors()
        {
            displaySummary = "display:none";
            AddressSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "addressInfo")
                {
                    ErrorsDesc += "<a href='#crbAddress'>" + Item.ErrorMessage + "</a>";
                    AddressSelection = Item.ErrorMessage;
                }
                
            }
        }
        private CitizenDataResponse GetCitizenData()
        {
            CitizenDataResponse Res = new CitizenDataResponse();
            var lang = "";
            if (Thread.CurrentThread.CurrentUICulture.Name == "el-GR")
            {
                lang = "el";
            }
            else
            {
                lang = "en";
            }
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var apiUrl = "api/v1/MoiCrmd/contact-info-mock/" + lang;
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            if (token == null)
            {
                token = HttpContext.GetTokenAsync("access_token").Result ?? "";
                HttpContext.Session.SetObjectAsJson("access_token", token, authTime);
            }
            string response;
            CitizenDataResponse _citizenPersonalDetails;
            try
            {
                response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "", token);
                _citizenPersonalDetails = JsonConvert.DeserializeObject<CitizenDataResponse>(response);
            }
            catch
            {
                _logger.LogError("Could not get valid response from " + apiUrl);
                _citizenPersonalDetails = new CitizenDataResponse();
                response = "";
            }
            if (_citizenPersonalDetails.succeeded & _citizenPersonalDetails.data != null)
            {
                Res = _citizenPersonalDetails;

            }
            else
            {
                Res = new CitizenDataResponse();
            }

            return Res;
        }
        #endregion
        public IActionResult OnGet()
        {
            //Check if no Citize details loaded
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res;
            if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            {
              res=GetCitizenData();
                if (res.succeeded == false)
                {
                  return RedirectToPage("/ServerError");
                }
                else
                {
                    //if loaded set in session
                    HttpContext.Session.SetObjectAsJson("PersonalDetails", res, authTime);
                }
            }
            else
            {
                //if we need to get it from session
                res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            }
            //Set address info to model class
            address_select.addressInfo = res.data.addressInfo;
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime);
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_civil)
                {
                    option1 = "true";
                    option2 = "false";
                } else
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
            if (crbAddress == "1")
            {
                address_select.use_from_civil = true;
                address_select.use_other = false;
            } else if (crbAddress == "2")
            {
                address_select.use_from_civil = false;
                address_select.use_other = true;
            }
            else
            {
                address_select.use_from_civil = false;
                address_select.use_other = false;
            }
            //Re-assign defult adressInfo
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            address_select.addressInfo = citizen_data.data.addressInfo;
           //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(address_select);
            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState, "address_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(result);
                return Page();
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("AddressSelect");
            HttpContext.Session.SetObjectAsJson("AddressSelect", address_select, authTime);
            //Finally redirect
            if (review)
            {
                if (address_select.use_other)
                {
                    return RedirectToPage("/AddressEdit", new { review = "true" });
                } else
                {
                    return RedirectToPage("/ReviewPage", null, "RedirectTarget");
                }
            } 
            else
            {
                if (address_select.use_other)
                {
                  return  RedirectToPage("/AddressEdit", null, "RedirectTarget");
                } 
                else
                {
                  return  RedirectToPage("/Mobile",null, "RedirectTarget");
                }
            }
           
        }
    }
}
