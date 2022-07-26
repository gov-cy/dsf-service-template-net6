using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace dsf_service_template_net6.Controllers
{
    public class CitizenController : Controller
    {
        private CitizenDataResponse _citizenPersonalDetails = new CitizenDataResponse();
        private readonly ILogger<CitizenController> _logger;
        private IMyHttpClient _client { get; set; }
        private IConfiguration _configuration;
        public CitizenController(IMyHttpClient client, IConfiguration configuration, ILogger<CitizenController> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
       {
        return View();
       }
       [Authorize]
       public IActionResult GetPersonalData(string currentLanguage,string returnUrl)
       {
            bool isPersonalDataRetrieve = true;
            
            //First check if user personal data have already being retrieve
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;

            //Call Api 
            //call the mock Api
            var apiUrl = "api/v1/MoiCrmd/contact-info-mock/" + currentLanguage;
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            try
            {
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
             catch
            {
                _logger.LogError("Could not get valid response from " + apiUrl);
                isPersonalDataRetrieve = false;
            }
           
            if (isPersonalDataRetrieve)
            {
                //Store Data in session
                //Set Data from CivilRegistry

                HttpContext.Session.SetObjectAsJson("PersonalDetails", _citizenPersonalDetails, authTime);
                return  RedirectToPage("/" + returnUrl);
            }
            else
            {
               return RedirectToPage("/ServerError");
            }

        }
    }
}
