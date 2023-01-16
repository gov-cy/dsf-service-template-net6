using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dsf.Service.Template.Services
{
    public interface IMyHttpClient
    {
        string MyHttpClientGetRequest(string baseUrl, string endpoint, string contentType, string accessToken="");
        string MyHttpClientPostRequest(string baseUrl, string endpoint, string contentType, string request, string accessToken = "");
    }
    public class MyHttpClient : IMyHttpClient
    {
        private  string ErrorLog = "MyHttpClient - MyHttpClientGetRequest: ";
        private  readonly IConfiguration _configuration;
        private readonly ILogger<MyHttpClient> _logger;

        public MyHttpClient(IConfiguration configuration, ILogger<MyHttpClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string MyHttpClientGetRequest(string baseUrl, string endpoint, string contentType, string accessToken = "")
        {
            string? ret = "";

            try
            {                

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                HttpClientHandler httpClientHandler = new()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                HttpClient httpClient = new(httpClientHandler)
                {
                    BaseAddress = new Uri(baseUrl)
                };

                httpClient.DefaultRequestHeaders.Add("client-key", _configuration["client-key"]);
                httpClient.DefaultRequestHeaders.Add("service-id", "DsfMock");
                httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                //httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["Ocp-Apim-Subscription-Key"]);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    //Include the Bearer Token
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                Task<HttpResponseMessage> response = httpClient.GetAsync(endpoint);
                response.Wait(TimeSpan.FromSeconds(10));

                if (response.IsCompleted)
                {
                    if (response.Result.StatusCode == HttpStatusCode.OK)
                    {
                        ret = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        ret = response.Result.Content.ToString();
                    }
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorLog + ex.ToString());

                ret = ex.Message;
            }

            return ret!;
        }

        public string MyHttpClientPostRequest(string baseUrl, string endpoint, string contentType, string request, string accessToken = "")
        {
            string? ret = "";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                HttpClientHandler httpClientHandler = new()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                HttpClient httpClient = new(httpClientHandler)
                {
                    BaseAddress = new Uri(baseUrl)
                };

                httpClient.DefaultRequestHeaders.Add("client-key", _configuration["client-key"]);
                httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    //Include the Bearer Token
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                HttpContent content = new StringContent(request, Encoding.UTF8, contentType);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                Task<HttpResponseMessage> response = httpClient.PostAsync(endpoint, content);
                response.Wait(TimeSpan.FromSeconds(10));

                if (response.IsCompleted)
                {
                    if (response.Result.StatusCode == HttpStatusCode.OK)
                    {
                        ret = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        ret = response.Result.Content.ToString();
                    }
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                ErrorLog = ErrorLog.Replace("Get", "Post");
                _logger.LogError(ErrorLog + ex.ToString());

                ret = ex.Message;
            }

            return ret!;
        }
    }
}