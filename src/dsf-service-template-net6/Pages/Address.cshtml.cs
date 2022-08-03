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
    public class AddressModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string crbAddress { get; set; }
        [BindProperty]
        public string option1 => (string)TempData[nameof(option1)];
        [BindProperty]
        public string option2 => (string)TempData[nameof(option2)];
        [BindProperty]
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string AddressSelection { get; set; } = "";
        //Dependancy injection Variables
        private readonly ILogger<AddressModel> _logger;
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<AddressSelect> _validator;
        //Object for session data, will be set internally
        //from web form variables
        public AddressSelect address_select;
        #endregion
        #region "Custom Methods"
        //Constructor
        public AddressModel(IValidator<AddressSelect> validator, IMyHttpClient client, IConfiguration config, ILogger<AddressModel> logger)
        {      _client = client;
                _configuration = config;
                _validator = validator;
                address_select = new AddressSelect();
                _logger = logger;
        }
       //Use to show error messages if web form has errors
        bool ShowErrors()
        {
            if (HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult") != null)
            
            {
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "address_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(res);
                return true;
            } else
            {
                return false;
            }
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
       //api call to get personal data from Civil Registry
        private CitizenDataResponse GetCitizenData(string lang)
        {
            CitizenDataResponse Res = new CitizenDataResponse();

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
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res;
            //If coming fromPost
            if (!ShowErrors())
            {
                //Load the web form
                //get the current lang
                var lang = "";
                if (Thread.CurrentThread.CurrentUICulture.Name == "el-GR")
                {
                    lang = "el";
                }
                else
                {
                    lang = "en";
                }
                //if user is already login then the Citizen controller
                //has not being executed.
                //Call the citizen personal details from civil registry  
                res = GetCitizenData(lang);
                if (res.succeeded == false)
                {
                    return RedirectToPage("/ServerError");
                }
                else
                {
                    //if the user is already login and not passed from login, set in session
                    HttpContext.Session.SetObjectAsJson("PersonalDetails", res, authTime);

                }
                
                
            }
            //Bind Data
            res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            //Set address info to model class
            address_select.addressInfo = res.data.addressInfo;
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime);
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_civil)
                {
                    TempData["option1"] = "true";
                    TempData["option2"] = "false";
                }
                else
                {
                    TempData["option1"] = "false";
                    TempData["option2"] = "true";
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
            }
            else if (crbAddress == "2")
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
                HttpContext.Session.SetObjectAsJson("valresult",  result);
                return RedirectToPage("Address");
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("AddressSelect");
            HttpContext.Session.SetObjectAsJson("AddressSelect", address_select, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect
            if (review)
            {
                if (address_select.use_other)
                {
                    return RedirectToPage("/AddressEdit", null,new { review = "true" }, "mainContainer");
                }
                else
                {
                    return RedirectToPage("/ReviewPage", null, "mainContainer");
                }
            }
            else
            {
                if (address_select.use_other)
                {
                    return RedirectToPage("/AddressEdit", null, "mainContainer");
                }
                else if (string.IsNullOrEmpty(citizen_data.data.mobile))
                {
                    return RedirectToPage("/MobileEdit", null, "mainContainer");
                }
                else
                {
                    return RedirectToPage("/Mobile", null, "mainContainer");
                }
            }

        }
    }

}
