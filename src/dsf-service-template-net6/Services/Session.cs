using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services.Model;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;

namespace dsf_service_template_net6.Services
{
    public interface IUserSession
    {
        public List<HistoryItem>? GetHistrory(); 
        public void SetHistrory(List<HistoryItem> Data);
        public List<SectionInfo>? GetNavLink();
        public void SetNavLink(List<SectionInfo> Data);
        public ValidationResult GetUserValidationResults();
        public void SetUserValidationResults(ValidationResult Result);
        public string? GetAccessToken();
        public void SetAccessToken(string AccessToken);
        public string? GetIdToken();
        public void SetIdToken(string IdToken);
        public ContactInfoResponse? GetUserPersonalData();
        public void SetUserPersonalData(ContactInfoResponse Data);
        public EmailSection? GetUserEmailData();
        public void SetUserEmailData(EmailSection Data);
        public MobileSection? GetUserMobileData();
        public void SetUserMobileData(MobileSection Data);
    }
    public class UserSession : IUserSession
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private string GetAuthTime()
        {
            return _httpContextAccessor!.HttpContext!.User!.Claims!.First(c => c.Type == "auth_time")!.Value;
        }
        public UserSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        } 
        public List<HistoryItem>? GetHistrory()
        {
            return _httpContextAccessor!.HttpContext!.Session.GetObjectFromJson<List<HistoryItem>>("History") ?? new List<HistoryItem>(); ;
        }
        public void SetHistrory(List<HistoryItem> Data)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("History", Data);
        }
        public List<SectionInfo>? GetNavLink()
        {
            return _httpContextAccessor!.HttpContext!.Session.GetObjectFromJson<List<SectionInfo>>("NavList");
        }
        public void SetNavLink(List<SectionInfo> Data)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("NavList", Data);
        }
        public string? GetAccessToken()
        {
            return _httpContextAccessor?.HttpContext?.GetTokenAsync("access_token")?.Result;
        }
        public void SetAccessToken(string AccessToken)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("access_token", AccessToken, GetAuthTime());
        }
        public string? GetIdToken()
        {
            return _httpContextAccessor?.HttpContext?.GetTokenAsync("id_token")?.Result;
        }
        public void SetIdToken(string IdToken)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("id_token", IdToken, GetAuthTime());
        }
        public ValidationResult GetUserValidationResults()
        {
            return _httpContextAccessor!.HttpContext!.Session.GetObjectFromJson<ValidationResult>("valresult");
        }
        public void SetUserValidationResults(ValidationResult Result)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("valresult", Result);
        }
        public ContactInfoResponse? GetUserPersonalData()
        {
            return _httpContextAccessor?.HttpContext?.Session.GetObjectFromJson<ContactInfoResponse>("PersonalDetails", GetAuthTime());
        }
        public void SetUserPersonalData(ContactInfoResponse Data)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("PersonalDetails", Data, GetAuthTime());
        }
        public EmailSection GetUserEmailData()
        {
            return _httpContextAccessor!.HttpContext!.Session.GetObjectFromJson<EmailSection>("EmailSection", GetAuthTime());
        }
        public void SetUserEmailData(EmailSection Data)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("EmailSection", Data, GetAuthTime());
        }
        public MobileSection GetUserMobileData()
        {
            return _httpContextAccessor!.HttpContext!.Session.GetObjectFromJson<MobileSection>("MobileSection", GetAuthTime());
        }
        public void SetUserMobileData(MobileSection Data)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("MobileSection", Data, GetAuthTime());
        }

    }
}
