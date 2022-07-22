using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class ReviewPageModel : PageModel
    {
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        public ReviewPageModel(IConfiguration configuration, IMyHttpClient client)
        {
            _client = client;
            _configuration = configuration;
        }
        public CitizenDataResponse _citizenPersonalDetails = new CitizenDataResponse();
        public ApplicationRequest _application = new ApplicationRequest();
        public string currentLanguage;
        //Data retrieve from other pages
        private Addressinfo[] ret_address;
        private string ret_email = string.Empty;
        private string ret_mobile = string.Empty;
        public IActionResult OnGet()
        {
            if (Thread.CurrentThread.CurrentUICulture.Name == "el-GR")
            {
                currentLanguage = "el";
            }
            else
            {
                currentLanguage = "en";
            }
           
            //Set access token
            SetAccessToken();
            bool ret = GetCitizenData();
            if (!ret)
            {
                return RedirectToPage("/Index");
            }
           
            //Set Data from CivilRegistry
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            HttpContext.Session.SetObjectAsJson("PersonalDetails", _citizenPersonalDetails, authTime);
            //Set Data from journey pages
            bool proceed = SetUserJourneyData();
            return Page();
        }
        public IActionResult OnPostApplicationSubmit(string applicationReference, string returnUrl = null)
        {   //Save the Application
            //Set ApplicationRequest
            var  ret = SetApplication();
            if (!ret)
            {
                return RedirectToPage("/Error");
            }
            if (SubmitApplication())
                {
                    return RedirectToPage("/ApplicationResponse");
                }
                else
                {
                    return RedirectToPage("/Error");
                }
        }
        private void SetAccessToken()
        {
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            if (token == null)
            {
                //set token
                var value = HttpContext.GetTokenAsync("access_token").Result ?? "";
                HttpContext.Session.SetObjectAsJson("access_token", value, authTime);
            }

        }
       private bool SetUserJourneyData()
       {
            bool ret = true;
            ret_address = _citizenPersonalDetails.data.addressInfo;
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
            var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
            ret_email = SessionEmailEdit.email;
            ret_mobile = SessionMobEdit.mobile;
            return ret;
       }
        private bool SetApplication()
        {
            bool ret = true;
            _application.contactInfo.addressInfo = ret_address;
            if (string.IsNullOrEmpty(ret_email) || string.IsNullOrEmpty(ret_mobile))
            {
               ret = false;
            }else
            {
                _application.contactInfo.email = ret_email;
                _application.contactInfo.mobile = ret_mobile;
            }
            _application.reference = Guid.NewGuid().ToString();
           
            _application.contactInfo.emailVerified = true;
            _application.contactInfo.mobileVerified = true;
                       
           return ret;
        }
        private bool GetCitizenData()
        {
            bool isPersonalDataRetrieve = true;
          
           //First check if user personal data have already being retrieve
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            
                //Call Api 
                //call the mock Api
                var apiUrl = "api/v1/MoiCrmd/contact-info-mock/" + currentLanguage;
                var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
                var response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "", token);
                if (response != null)     
                {
                    _citizenPersonalDetails = JsonConvert.DeserializeObject<CitizenDataResponse>(response);
                    if (_citizenPersonalDetails == null)
                    {
                        isPersonalDataRetrieve = false;
                    }
                    else if (!_citizenPersonalDetails.succeeded)
                    {
                        isPersonalDataRetrieve = false;
                    } 
                }
                else
                {
                    isPersonalDataRetrieve = false;
                }
               
            
            return isPersonalDataRetrieve;
        }
        private bool SubmitApplication()
        {           
            bool ret = false;
            string jsonString = JsonConvert.SerializeObject(_application);
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            var response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], "api/v1/MoiCrmd/contact-info-submission-mock", "application/json", jsonString,token);

            if (response != null)
            {
                var res = JsonConvert.DeserializeObject<ApplicationResponse>(response);

                if (res != null)
                {
                    HttpContext.Session.SetObjectAsJson("ref_no", res.data, authTime);
                }

                ret = true;
            }

            return ret;
        }
    }
}
