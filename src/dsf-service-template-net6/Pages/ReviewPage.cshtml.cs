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
        public Addressinfo[] ret_address;
        public string ret_email = string.Empty;
        public string ret_mobile = string.Empty;
        public IActionResult OnGet()
        {
                    
            //Set access token
            SetAccessToken();
               
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
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            _citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            ret_address = _citizenPersonalDetails.data.addressInfo;
           
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
            var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
            ret_email = SessionEmailEdit.email;
            ret_mobile = SessionMobEdit.mobile;
            return ret;
       }
        private bool SetApplication()
        {
            bool ret = true;
            bool isDataRetrieve =SetUserJourneyData();
            if (isDataRetrieve)
            {
                _application.contactInfo = new Contactinfo();
                _application.contactInfo.addressInfo = new Addressinfo[] {};
                _application.contactInfo.addressInfo = ret_address;
                if (string.IsNullOrEmpty(ret_email) || string.IsNullOrEmpty(ret_mobile))
                {
                  ret = false;
                }else
                {
                _application.contactInfo.email = ret_email;
                _application.contactInfo.mobile = ret_mobile;
                }
            }
         
            _application.reference = Guid.NewGuid().ToString();
           
            _application.contactInfo.emailVerified = true;
            _application.contactInfo.mobileVerified = true;
                       
           return ret;
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
