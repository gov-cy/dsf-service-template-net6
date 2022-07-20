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
        public string currentLanguage;
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
          //  FormatAddress();
            return Page();
        }
        private void SetAccessToken()
        {        
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
                if (token==null)
                {
                    //set token
                    
                    var value = HttpContext.GetTokenAsync("access_token").Result ?? "";
                    HttpContext.Session.SetObjectAsJson("access_token", value, authTime);
                } 
                   
        }
        //private void FormatAddress()
        //{
        //    foreach (Addressinfo item in _citizenPersonalDetails.data.addressInfo)
        //    {
        //        item.addressText = String.Format(item.addressText);
        //        item.addressText=item.item.name + " " + item.item.street.streetNumber 
        //    }
        //}
        private bool GetCitizenData()
        {
            bool isPersonalDataRetrieve = true;
          
           //First check if user personal data have already being retrieve
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                _citizenPersonalDetails = citizenPersonalDetails;
            }
            else
            {
                //Call Api 
                //get uniqueid
                //  var id = User.Claims.First(p => p.Type == "unique_identifier").Value;
                //call the mock Api
                var apiUrl = "v1/MoiCrmd/contact-info-mock/" + currentLanguage;
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
               
            }
            return isPersonalDataRetrieve;
        }
    }
}
