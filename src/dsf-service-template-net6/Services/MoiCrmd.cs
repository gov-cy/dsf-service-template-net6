namespace dsf_service_template_net6.Services
{
    using dsf_service_template_net6.Data.Models;
    using dsf_service_template_net6.Extensions;
    using Newtonsoft.Json;
    public interface IMoiCrmd
    {
        CitizenDataResponse GetCitizenData(string language, string accesstoken);
        ApplicationResponse SubmitApplication(ApplicationRequest req, string accesstoken);


    }
    public class MoiCrmd : IMoiCrmd
    {
        private IConfiguration _configuration;
        private readonly ILogger<MoiCrmd> _logger;
        private IMyHttpClient _client;

        public MoiCrmd(IConfiguration configuration, ILogger<MoiCrmd> logger, IMyHttpClient client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
        }
       

        public CitizenDataResponse GetCitizenData(string language, string accesstoken)
        {
            CitizenDataResponse dataResponse = new();
            var apiUrl = $"api/v1/MoiCrmd/contact-info-mock/{language}";
            string response = null;
            try
            {
                response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "application/json", accesstoken);
               
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new CitizenDataResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<CitizenDataResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new CitizenDataResponse();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    _logger.LogError("GetCitizenData - Exception" + ex.ToString());
                    dataResponse = new CitizenDataResponse();
                }

                if (dataResponse.succeeded)
                {
                   
                    return dataResponse;
                }
                if (dataResponse.errorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new CitizenDataResponse();
                    rsp.errorCode = dataResponse.errorCode;
                    rsp.errorMessage = dataResponse.errorMessage;
                    dataResponse = rsp;
                }
            }
            return dataResponse;
        }

        public ApplicationResponse SubmitApplication(ApplicationRequest req, string accesstoken)
        {
            ApplicationResponse dataResponse = new();
            var apiUrl = "api/v1/MoiCrmd/contact-info-submission-mock";
            string jsonString = JsonConvert.SerializeObject(req);
            string response = null;
            try
            {
                response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], apiUrl, "application/json", jsonString, accesstoken);
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new ApplicationResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<ApplicationResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new ApplicationResponse();
                    }
                }
                catch
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    dataResponse = new ApplicationResponse();
                }
                if (dataResponse.errorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new ApplicationResponse();
                    rsp.errorCode = dataResponse.errorCode;
                    rsp.errorMessage = dataResponse.errorMessage;
                    dataResponse = rsp;
                }
            }

            return dataResponse;
        }
    }
}
