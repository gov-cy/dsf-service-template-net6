using DSF.AspNetCore.Web.Template.Services.Model;
using Newtonsoft.Json;

namespace DSF.AspNetCore.Web.Template.Services
{
    public interface ICommonApis
    {
        bool IsMobileValid(string Mobile);
        bool IsEmailValid(string Email);     
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

        public bool IsMobileValid(string Mobile)
        {
            bool result = !string.IsNullOrEmpty(Mobile);
            if (result)
            {
                Mobile = Mobile.StartsWith("003579") && Mobile.Substring(6).Length == 7 ? Mobile.Substring(5) : Mobile;
                bool isCyprusPhone = Mobile.Length == 8;

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

        public bool IsEmailValid(string Email)
        {
            bool result = (!string.IsNullOrEmpty(Email));
            if (result)
            {
                //Call Api
                string urlToValidate = "api/v1/Validation/email-validation/" + Email;
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
