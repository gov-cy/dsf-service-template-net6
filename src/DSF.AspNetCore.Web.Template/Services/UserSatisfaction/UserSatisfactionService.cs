namespace DSF.AspNetCore.Web.Template.Services.UserSatisfaction;

using DSF.AspNetCore.Web.Template.Services.Model;
using DSF.AspNetCore.Web.Template.Services.UserSatisfaction.Data;
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

    private readonly IMyHttpClient _client;
    private readonly ILogger<Contact> _logger;
    private readonly IConfiguration _configuration;

    public UserSatisfactionService
    (
        IMyHttpClient client,
        ILogger<Contact> logger,
        IConfiguration configuration
    )
    {
        _client = client;
        _logger = logger;
        _configuration = configuration;
    }

    public BaseResponse<string> SubmitUserSatisfaction(UserSatisfactionServiceRequest request)
    {
        BaseResponse<string>? dataResponse = new();

        string jsonString = JsonConvert.SerializeObject(request);
        string apiUrl = string.IsNullOrEmpty(request.AccessToken)
            ? "/api/v1/UserFeedback/feedback-record"
            : "/api/v1/UserFeedback/feedback-record-authorized";

        var response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], apiUrl, "application/json",
            jsonString, request.AccessToken);

        if (response != null)
        {
            try
            {
                dataResponse = JsonConvert.DeserializeObject<BaseResponse<string>>(response);
                if (dataResponse == null)
                {
                    _logger.LogError("Received Null response from {apiUrl}", apiUrl);
                    dataResponse = new BaseResponse<string>();
                }
            }
            catch (Exception ex)
            {
                var exString = ex.ToString();
                _logger.LogError("Could not get valid response from {apiUrl}", apiUrl);
                _logger.LogError("GetCitizenData - Exception {exString}", exString);
                dataResponse = new BaseResponse<string>();
            }

            if (dataResponse.Succeeded)
            {

                return dataResponse;
            }
            if (dataResponse.ErrorCode != 0)
            {
                _logger.LogInformation("Could not get valid response from {apiUrl}", apiUrl);
                var rsp = new BaseResponse<string>
                {
                    ErrorCode = dataResponse.ErrorCode,
                    ErrorMessage = dataResponse.ErrorMessage
                };
                dataResponse = rsp;
            }
        }
        return dataResponse;
    }
}