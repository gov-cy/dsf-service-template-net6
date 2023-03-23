namespace DSF.AspNetCore.Web.Template.Services.UserSatisfaction;

using global::DSF.AspNetCore.Web.Template.Services.Model;
using global::DSF.AspNetCore.Web.Template.Services.UserSatisfaction.Data;
using Newtonsoft.Json;

public class UserSatisfactionService : IUserSatisfactionService
{
    public class UserSatisfactionStatus : Enumeration
    {
        public static readonly UserSatisfactionStatus SUCCESS = new(0, nameof(SUCCESS));
        public static readonly UserSatisfactionStatus API_EXCEPTION_ERROR = new(-500, nameof(API_EXCEPTION_ERROR));
        public static readonly UserSatisfactionStatus GENERAL_ERROR = new(-501, nameof(GENERAL_ERROR));

        public UserSatisfactionStatus(int id, string name) : base(id, name) { }
    }

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMyHttpClient _client;
    private readonly IConfiguration _systemConfig;
    private readonly ILogger<Contact> _logger;

    public UserSatisfactionService
    (
        IHttpContextAccessor httpContextAccessor,
        IMyHttpClient client,
        IConfiguration config,
        ILogger<Contact> logger
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _client = client;
        _systemConfig = config;
    }

    public BaseResponse<string> SubmitUserSatisfaction(UserSatisfactionServiceRequest request)
    {
        BaseResponse<string>? dataResponse = new();

        string jsonString = JsonConvert.SerializeObject(request);
        string apiUrl = string.IsNullOrEmpty(request.AccessToken)
            ? "/api/v1/UserFeedback/feedback-record"
            : "/api/v1/UserFeedback/feedback-record-authorized";

        var response = _client.MyHttpClientPostRequest("https://dsf-api-dev.dmrid.gov.cy/", apiUrl, "application/json",
            jsonString, request.AccessToken, "DsfMlsiSisCbg");
        response = _client.MyHttpClientPostRequest("https://dsf-api-dev.dmrid.gov.cy/", apiUrl, "application/json",
    jsonString, request.AccessToken, "DsfMoiElectionsEcr");
        response = _client.MyHttpClientPostRequest("https://dsf-api-dev.dmrid.gov.cy/", apiUrl, "application/json",
    jsonString, request.AccessToken, "DsfMoiCrmdCdu");
        if (response != null)
        {
            try
            {
                dataResponse = JsonConvert.DeserializeObject<BaseResponse<string>>(response);
                if (dataResponse == null)
                {
                    _logger.LogError("Received Null response from " + apiUrl);
                    dataResponse = new BaseResponse<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get valid response from " + apiUrl);
                _logger.LogError("GetCitizenData - Exception" + ex.ToString());
                dataResponse = new BaseResponse<string>();
            }

            if (dataResponse.Succeeded)
            {

                return dataResponse;
            }
            if (dataResponse.ErrorCode != 0)
            {
                _logger.LogInformation("Could not get valid response from " + apiUrl);
                var rsp = new BaseResponse<string>();
                rsp.ErrorCode = dataResponse.ErrorCode;
                rsp.ErrorMessage = dataResponse.ErrorMessage;
                dataResponse = rsp;
            }
        }
        return dataResponse;
    }
}