using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
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

        public IActionResult OnGet()
        {
            bool ret = GetCitizenData();
            if (!ret)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }
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
                var apiUrl = "contact-info-mock/en";
                var response = _client.MyHttpClientGetRequest("https://apimocha.com/dsf-test-api/", apiUrl, "");
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
               
            }
            return isPersonalDataRetrieve;
        }
    }
}
