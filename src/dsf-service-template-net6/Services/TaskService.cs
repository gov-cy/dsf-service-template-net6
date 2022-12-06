namespace dsf_service_template_net6.Services
{  
    using dsf_service_template_net6.Services.Model;
    using Newtonsoft.Json;
    public interface ITasks
    {
        TasksResponse GetAllTasks(string accesstoken);
        TasksResponse SubmitTask(Task req, string accesstoken);


    }
    public class Tasks : ITasks
    {
        private IConfiguration _configuration;
        private readonly ILogger<Tasks> _logger;
        private IMyHttpClient _client;

        public Tasks(IConfiguration configuration, ILogger<Tasks> logger, IMyHttpClient client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
        }
       

        public TasksResponse GetAllTasks(string accesstoken)
        {
            TasksResponse dataResponse = new();
            var apiUrl = "api/v1/TodoItems";
            string response = null;
            try
            {
                response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "application/json", accesstoken);
               
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new TasksResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<TasksResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new TasksResponse();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    _logger.LogError("GetCitizenData - Exception" + ex.ToString());
                    dataResponse = new TasksResponse();
                }

                if (dataResponse.succeeded)
                {
                   
                    return dataResponse;
                }
                if (dataResponse.errorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new TasksResponse();
                    rsp.errorCode = dataResponse.errorCode;
                    rsp.errorMessage = dataResponse.errorMessage;
                    dataResponse = rsp;
                }
            }
            return dataResponse;
        }

        public TasksResponse SubmitTask(Task req, string accesstoken)
        {
            TasksResponse dataResponse = new();
            var apiUrl = "api/v1/TodoItems";
            string jsonString = JsonConvert.SerializeObject(req);
            string response = null;
            try
            {
                response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], apiUrl, "application/json", jsonString, accesstoken);
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new TasksResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<TasksResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new TasksResponse();
                    }
                }
                catch
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    dataResponse = new TasksResponse();
                }
                if (dataResponse.errorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new TasksResponse();
                    rsp.errorCode = dataResponse.errorCode;
                    rsp.errorMessage = dataResponse.errorMessage;
                    dataResponse = rsp;
                }
            }

            return dataResponse;
        }
    }
}
