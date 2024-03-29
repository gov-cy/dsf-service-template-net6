﻿using DSF.AspNetCore.Web.Template.Services.Model;
using Newtonsoft.Json;

namespace DSF.AspNetCore.Web.Template.Services
{ 
    public interface IContact
    {
        ContactInfoResponse GetContact(string accesstoken);
        ContactInfoResponse SubmitContact(ContactInfo req, string accesstoken);


    }
    public class Contact : IContact
    {
        private IConfiguration _configuration;
        private readonly ILogger<Contact> _logger;
        private IMyHttpClient _client;

        public Contact(IConfiguration configuration, ILogger<Contact> logger, IMyHttpClient client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
        }
   

        public ContactInfoResponse GetContact(string accesstoken)
        {
            ContactInfoResponse? dataResponse = new();
            var apiUrl = "api/v1/ContactInfo";
            string? response = null;
            try
            {
                response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "application/json", accesstoken);
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new ContactInfoResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<ContactInfoResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new ContactInfoResponse();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    _logger.LogError("GetCitizenData - Exception" + ex.ToString());
                    dataResponse = new ContactInfoResponse();
                }

                if (dataResponse.Succeeded)
                {
                   
                    return dataResponse;
                }
                if (dataResponse.ErrorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new ContactInfoResponse();
                    rsp.ErrorCode = dataResponse.ErrorCode;
                    rsp.ErrorMessage = dataResponse.ErrorMessage;
                    dataResponse = rsp;
                }
            }
            return dataResponse;
        }

        public ContactInfoResponse SubmitContact( ContactInfo req, string accesstoken)
        {
            ContactInfoResponse? dataResponse = new();
            var apiUrl = "api/v1/ContactInfo";
            string jsonString = JsonConvert.SerializeObject(req);
            string? response = null;
            try
            {
                response = _client.MyHttpClientPostRequest(_configuration["ApiUrl"], apiUrl, "application/json", jsonString, accesstoken);
            }
            catch
            {
                _logger.LogError("Fail to call Api for " + apiUrl);
                dataResponse = new ContactInfoResponse();
            }
            if (response != null)
            {
                try
                {
                    dataResponse = JsonConvert.DeserializeObject<ContactInfoResponse>(response);
                    if (dataResponse == null)
                    {
                        _logger.LogError("Received Null response from " + apiUrl);
                        dataResponse = new ContactInfoResponse();
                    }
                }
                catch
                {
                    _logger.LogError("Could not get valid response from " + apiUrl);
                    dataResponse = new ContactInfoResponse();
                }
                if (dataResponse.ErrorCode != 0)
                {
                    _logger.LogInformation("Could not get valid response from " + apiUrl);
                    var rsp = new ContactInfoResponse
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
}
