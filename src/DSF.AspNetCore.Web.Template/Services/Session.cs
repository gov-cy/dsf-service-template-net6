using DSF.AspNetCore.Web.Template.Data.Models;
using DSF.AspNetCore.Web.Template.Extensions;
using DSF.AspNetCore.Web.Template.Services.Model;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;

namespace DSF.AspNetCore.Web.Template.Services
{
    public interface IUserSession
    {
        public List<HistoryItem> GetHistrory(); 
        public void SetHistrory(List<HistoryItem> Data);
        public List<SectionInfo> GetNavLink();
        public void SetNavLink(List<SectionInfo> Data);
        public ValidationResult? GetUserValidationResults();
        public void SetUserValidationResults(ValidationResult Result);
        public string? GetAccessToken();
        public void SetAccessToken(string AccessToken);
        public string? GetIdToken();
        public void SetIdToken(string IdToken);
        public ContactInfoResponse? GetUserPersonalData();
        public void SetUserPersonalData(ContactInfoResponse Data);
        public EmailSection GetUserEmailData();
        public void SetUserEmailData(EmailSection Data);
        public MobileSection GetUserMobileData();
        public void SetUserMobileData(MobileSection Data);
       
        public ContactInfoResponse? GetUserApplResponse();
        public void SetUserApplResponse(ContactInfoResponse Data);

        public ContactInfo? GetUserApplRequest();
        public void SetUserApplRequest(ContactInfo Data);

        public string GetUserReferenceNumber();
        public void SetUserReferenceNumber(string ReferenceNumber);
    }
    public class UserSession : IUserSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
       
        public UserSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        } 
        public List<HistoryItem> GetHistrory()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<List<HistoryItem>>("History") ?? new();
        }
        public void SetHistrory(List<HistoryItem> Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("History", Data);
        }
        public List<SectionInfo> GetNavLink()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<List<SectionInfo>>("NavList") ?? new();
        }
        public void SetNavLink(List<SectionInfo> Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("NavList", Data);
        }
        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.GetTokenAsync("access_token")?.Result;
        }
        public void SetAccessToken(string AccessToken)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("access_token", AccessToken);
        }
        public string? GetIdToken()
        {
            return _httpContextAccessor.HttpContext?.GetTokenAsync("id_token")?.Result;
        }
        public void SetIdToken(string IdToken)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("id_token", IdToken);
        }
        public ValidationResult? GetUserValidationResults()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<ValidationResult>("valresult");
        }
        public void SetUserValidationResults(ValidationResult Result)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("valresult", Result);
        }
        public ContactInfoResponse? GetUserPersonalData()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<ContactInfoResponse>("PersonalDetails");
        }
        public void SetUserPersonalData(ContactInfoResponse Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("PersonalDetails", Data);
        }
        public EmailSection GetUserEmailData()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<EmailSection>("EmailSection") ?? new();
        }
        public void SetUserEmailData(EmailSection Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("EmailSection", Data);
        }
        public MobileSection GetUserMobileData()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<MobileSection>("MobileSection") ?? new();
        }
        public void SetUserMobileData(MobileSection Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("MobileSection", Data);
        }
        public ContactInfoResponse? GetUserApplResponse() 
        { 
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<ContactInfoResponse>("ApplRes");
        }
        public void SetUserApplResponse(ContactInfoResponse Data) 
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("ApplRes", Data);
        }

        public ContactInfo? GetUserApplRequest()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<ContactInfo>("ApplReq");
        }
        public void SetUserApplRequest(ContactInfo Data)
        {
            _httpContextAccessor.HttpContext?.Session.SetObjectAsJson("ApplReq", Data);
        }

        public string GetUserReferenceNumber()
        {
            return _httpContextAccessor.HttpContext?.Session.GetObjectFromJson<string>("ref_no") ?? string.Empty;
        }
        public void SetUserReferenceNumber(string ReferenceNumber)
        {
            _httpContextAccessor!.HttpContext!.Session.SetObjectAsJson("ref_no", ReferenceNumber);
        }
    }
}
