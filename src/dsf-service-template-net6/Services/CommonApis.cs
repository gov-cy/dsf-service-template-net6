using Dsf.Service.Template.Services;
using Dsf.Service.Template.Services.Model;
using Newtonsoft.Json;

namespace dsf_moi_election_catalogue.Services
{
    public interface ICommonApis
    {
        Boolean IsMobileValid(string Mobile);
        Boolean IsEmailValid(string Email);
      
    }
    public class CommonApis: ICommonApis
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommonApis> _logger;
        private readonly IMyHttpClient _client;
        public CommonApis(IConfiguration configuration, ILogger<CommonApis> logger, IMyHttpClient client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;

        }
        public Boolean IsMobileValid(string Mobile)
        {
            bool result = (!string.IsNullOrEmpty(Mobile));
            if (result)
            {

                Mobile = Mobile.StartsWith("00357") && Mobile.Substring(5).Length == 8 ? Mobile.Substring(5) : Mobile;

                bool isCyprusPhone = (Mobile.StartsWith("00357") && Mobile.Length == 8) || Mobile.Length == 8;

                //Call Api
                string urlToValidate = "api/v1/Validation/cy-mobile-number-validation/" + Mobile; 
                if (!isCyprusPhone)
                {
                    return false;
                }
                string? response;
                try
                {
                    response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], urlToValidate, "application/json");
                }
                catch
                {
                    _logger.LogError("Fail to call Api for " + urlToValidate);
                    return false;
                }
                if (response != null)
                {
                    MobValidationResp? resp;
                    try
                    {
                       resp = JsonConvert.DeserializeObject<MobValidationResp>(response);
                     
                    }
                    
                    catch (System.Text.Json.JsonException) // Invalid JSON
                    {
                        _logger.Log(LogLevel.Error, "Error Validate Mobile " + Mobile);
                        return false;
                    }
                    if (resp?.Succeeded == false)
                    {
                        return false;
                    }
                    
                }
            }
            return result;
        }
        public Boolean IsEmailValid(string Email)
        {
            bool result = (!string.IsNullOrEmpty(Email));
            if (result)
            {
                //Replace () if any
             

                //Call Api
                string urlToValidate = String.Empty;
               
               
                    urlToValidate = "api/v1/Validation/email-validation/" + Email;
                
                string? response = null;
                try
                {
                    response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], urlToValidate, "application/json");
                }
                catch
                {
                    _logger.LogError("Fail to call Api for " + urlToValidate);
                    return false;
                }
                if (response != null)
                {
                    EmailValidationResp? resp = new();
                    try
                    {
                        resp = JsonConvert.DeserializeObject<EmailValidationResp>(response);

                    }

                    catch (System.Text.Json.JsonException) // Invalid JSON
                    {
                        _logger.Log(LogLevel.Error, "Error Validate Mobile " + Email);
                        return false;
                    }
                    if (resp?.Succeeded==false)
                    {
                        return false;
                    }

                }
            }
            return result;
        }

    }
}
