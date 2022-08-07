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
    public class ReviewPageModel : BasePage
    {
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        public ReviewPageModel(IConfiguration configuration, IMyHttpClient client)
        {
            _client = client;
            _configuration = configuration;
        }
        #region "Variables"
        public CitizenDataResponse _citizenPersonalDetails = new CitizenDataResponse();
        public ApplicationRequest _application = new ApplicationRequest();
        public string currentLanguage;
        //Data retrieve from other pages
        public Addressinfo[] ret_address = Array.Empty<Addressinfo>();
        public string ret_email = string.Empty;
        public string ret_mobile = string.Empty;
        public bool useEmailEditOnly = false;
        public bool useMobileEditOnly = false;
        #endregion
        public IActionResult OnGet()
        {
            //Set back and Next Link
            SetLinks("ReviewPage", false);
            bool allow = AllowToProceed();
            if (!allow)
            {
              return RedirectToAction("LogOut", "Account");
            }
               
            //Set Data from journey pages
            bool proceed = SetUserJourneyData();
            if (!proceed)
            {
              return  RedirectToAction("LogOut", "Account");
            }
            return Page();
        }
        public IActionResult OnPostApplicationSubmit(string applicationReference, string returnUrl = null)
        {   //Save the Application
            //Set ApplicationRequest
            var  ret = SetApplication();
            if (!ret)
            {
                return RedirectToPage("/ServerError");
            }
            if (SubmitApplication())
                {
                    return RedirectToPage("/ApplicationResponse");
                }
                else
                {
                    return RedirectToPage("/ServerError");
                }
        }
        #region "Custom Methods"
          private bool AllowToProceed()
        {
            //Make sure the sequence has been kept
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

            if ((HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", authTime) == null) && (HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime) == null))
            {
                ret = false;
            }
            return ret;
        }
          private bool SetUserJourneyData()
          {
            bool ret = true;

            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            //first get Address Select
            _citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            var addressSelect = HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime);
            if (addressSelect.use_from_civil)
            {               
                ret_address = _citizenPersonalDetails.data.addressInfo;
            } else
            {
                //Wait for Alkis Session Storage
                var citizenEdit= HttpContext.Session.GetObjectFromJson<Addressinfo>("AddressEdit", authTime);
                List<Addressinfo> items = new();
                items.Add(citizenEdit); 
                ret_address= items.ToArray();
            }
            var mobSelect = HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime);
            if (mobSelect != null)
            {   
                if (mobSelect.use_from_civil)
                {
                ret_mobile = _citizenPersonalDetails.data.mobile;
                }
                else
                {
                var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
                ret_mobile = SessionMobEdit.mobile;
                }

            } else
            {
                //Directly to edit
                var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
                ret_mobile = SessionMobEdit.mobile;
                useMobileEditOnly = true;
            }
         
            var emailSelect = HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", authTime);
           if (emailSelect != null)
            {
                if (emailSelect.use_from_civil)
                {
                ret_email= _citizenPersonalDetails.data.email ?? User.Claims.First(c => c.Type == "email").Value; 
                } else 
                {
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
                ret_email = SessionEmailEdit.email;
                }
            }else
            {
                //Directrly to email edit
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
                ret_email = SessionEmailEdit.email;
                useEmailEditOnly = true;
            }
           
            
             return ret;
        }
          private bool SetApplication()
        {
            bool ret = true;
            bool isDataRetrieve = SetUserJourneyData();
            if (isDataRetrieve)
            {
                _application.contactInfo = new Contactinfo();
                _application.contactInfo.addressInfo = new Addressinfo[] { };
                _application.contactInfo.addressInfo = ret_address;
                if (string.IsNullOrEmpty(ret_email) || string.IsNullOrEmpty(ret_mobile))
                {
                    ret = false;
                }
                else
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
            try 
            {
                var response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], "api/v1/MoiCrmd/contact-info-submission-mock", "application/json", jsonString, token);

                if (response != null)
                {
                    var res = JsonConvert.DeserializeObject<ApplicationResponse>(response);

                    if (res != null)
                    {
                        HttpContext.Session.SetObjectAsJson("ref_no", res.data, authTime);
                    }

                    ret = true;
                }

                
            }
            catch
            {
                //Log
                ret=false;
            }
            return ret;
        }
        #endregion

    }
}
