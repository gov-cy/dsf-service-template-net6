using dsf_service_template_net6.Services.Model;
using System.Security.Claims;
namespace dsf_service_template_net6.Services
{
    using dsf_service_template_net6.Data.Models;
    using dsf_service_template_net6.Extensions;
    using Microsoft.AspNetCore.Http;
   
    public interface INavigation
    {
        public string BackLink { get; set; }
        public string NextLink { get; set; }
        public void SetLinks(string CurrPage, string Section, bool FromReview);
        public string GetBackLink(string curr, bool FromReview = false);
    }

    public class Navigation : INavigation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string BackLink { get; set; }
        public string NextLink { get; set; }
        private void setSectionPages(ClaimsPrincipal cp)
        {
            var authTime = cp.Claims.First(c => c.Type == "auth_time").Value;
            List<SectionInfo> list = new List<SectionInfo>() ;
            var citizen= _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails",authTime);
            SectionInfo item = new();
            item.Section = "Address";
            item.SectionOrder = 1;
            //if address from api call , show selection address,otherwise only edit page 
            item.PageType= (citizen?.data?.addressInfo?.Length > 0)? SectionType.SelectionAndInput: SectionType.InputOnly;
            list.Add(item);
            item = new();
            item.Section = "Email";
            item.SectionOrder = 2;
            //Always Select, for even API does not have email, we show email from user profile 
            item.PageType =  SectionType.SelectionAndInput;
            list.Add(item);
            item = new();
            item.Section = "Mobile";
            item.SectionOrder = 3;
            //Always Select, for even API does not have email, we show email from user profile 
            item.PageType = (!string.IsNullOrEmpty(citizen?.data?.mobile)) ? SectionType.SelectionAndInput : SectionType.InputOnly;
            list.Add(item);
            //Store List
            _httpContextAccessor.HttpContext!.Session.SetObjectAsJson("NavList",list);
        }
        public Navigation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var ListItem= _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<List<SectionInfo>>("NavList").First();
            BackLink = "/";
            setSectionPages(ClaimsPrincipal.Current);
            NextLink = (ListItem.PageType==SectionType.SelectionAndInput?"/address-selection": "/set-address");
        }

        

        public string GetBackLink(string curr, bool FromReview = false)
        {
            throw new NotImplementedException();
        }

        public void SetLinks(string CurrPage, string Section, bool FromReview)
        {
            throw new NotImplementedException();
        }
    }
}
