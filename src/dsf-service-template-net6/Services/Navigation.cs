using dsf_service_template_net6.Services.Model;
using System.Security.Claims;
namespace dsf_service_template_net6.Services
{
    using dsf_service_template_net6.Data.Models;
    using dsf_service_template_net6.Extensions;
    using Microsoft.AspNetCore.Http;
    using System.Collections;

    public enum FormSelection
    {
        Yes,
        No,
        NoSelection
    }
    public class HistoryItem
    {
        public string PageName { get; set; } = "";
        public bool Review { get; set; }

    }
    public interface INavigation
    {
        public string BackLink { get; set; }
        public string NextLink { get; set; }
        public string SetLinks(string currPage, string section, bool fromReview, string selectChoice);
        public string GetBackLink(string currPage, bool fromReview = false);
    }

    public class Navigation : INavigation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string BackLink { get; set; }
        public string NextLink { get; set; }
        private List<HistoryItem> History { get; set; } = new List<HistoryItem>();
        Dictionary<string, string> _routes = new()
        {
            { "/Email", "/email-selection" },
            { "/EmailEdit", "/set-email" },
            { "/Mobile", "/mobile-selection" },
            { "/MobileEdit", "/set-mobile" },
            { "/ReviewPage", "/review-page" },
        };

        private void AddHistoryLinks(string currPage, bool review)
        {
            HistoryItem historyItem = new HistoryItem();
            History = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<HistoryItem>>("History") ?? new List<HistoryItem>();
            int LastIndex = History.Count;
            if (LastIndex == 0)
            {
                historyItem.Review = review;
                historyItem.PageName = "/";
                History.Add(historyItem);
            }
            else if (History[LastIndex - 1].PageName != currPage)
            {
                //Add to History
                historyItem.Review = review;
                historyItem.PageName = currPage;
                History.Add(historyItem);
                //Set to memory

            }
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson("History", History);

        }
        private void setSectionPages(ClaimsPrincipal cp)
        {
            var authTime = cp.Claims.First(c => c.Type == "auth_time").Value;
            List<SectionInfo> list = new List<SectionInfo>() ;
            var citizen= _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails",authTime);
            //New Section
            SectionInfo section = new();
            section.Name = "Email";
            section.SectionOrder = 1;
            //Always Select, for even API does not have email, we show email from user profile 
            section.Type =  SectionType.SelectionAndInput;
            section.pages.Add("email-selection");
            section.pages.Add("set-email");
            list.Add(section);
           //New Section
            section = new();
            section.Name = "Mobile";
            section.SectionOrder = 2;
            //Always Select, for even API does not have email, we show email from user profile 
            section.Type = (!string.IsNullOrEmpty(citizen?.data?.mobile)) ? SectionType.SelectionAndInput : SectionType.InputOnly;
            if (section.Type == SectionType.InputOnly)
            {
                section.pages.Add("set-mobile");
            }
            else
            {
                section.pages.Add("mobile-selection");
                section.pages.Add("set-mobile");
            }
            list.Add(section);
            //Store List
            _httpContextAccessor.HttpContext!.Session.SetObjectAsJson("NavList",list);
        }
        public Navigation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var ListItem = _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<List<SectionInfo>>("NavList")?.First();
            if (ListItem == null)
            {
                BackLink = "/";

                setSectionPages(_httpContextAccessor?.HttpContext?.User);
                ListItem = _httpContextAccessor.HttpContext!.Session.GetObjectFromJson<List<SectionInfo>>("NavList").First();
                NextLink = "/" + ListItem.pages.First();
            }
            
        }

        public string GetBackLink(string currPage, bool fromReview = false)
        {
            History = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<HistoryItem>>("History");
            if (!fromReview)
            {
                AddHistoryLinks(currPage, fromReview);
            }
            HistoryItem Item = History.Find(x => x.PageName == currPage && x.Review == fromReview);
            HistoryItem PrevItem = null;
            //if not found get the previous
            if (Item == null)
            {
                bool Found = History.Exists(x => x.PageName == currPage);
                if (Found)
                {
                    PrevItem = (History.Count > 1 ? History[History.Count - 2] : History[History.Count - 1]);
                }
                else
                {
                    //Just show Previous
                    PrevItem = (History.Count > 1 ? History[History.Count - 1] : History[0]);
                }
                if (PrevItem == null)
                {
                    Item = new HistoryItem();
                    Item.PageName = "/";
                    Item.PageName = (Item.Review ? Item.PageName + "?review=true" : Item.PageName);
                    Item.Review = fromReview;
                    BackLink = Item.PageName;

                    return Item.PageName;
                }
                else
                {
                    PrevItem.PageName = (PrevItem.Review ? PrevItem.PageName + "?review=true" : PrevItem.PageName);
                    BackLink = PrevItem.PageName;
                    return PrevItem.PageName;
                }

                //First time set again

            }
            //Return item
            else
            {
                int index = History.FindIndex(x => x == Item);
                //set the prev item 
                Item = History[index - 1];
                Item.PageName = (Item.Review ? Item.PageName + "?review=true" : Item.PageName);
                BackLink = Item.PageName;
                return Item.PageName;

            }
           
        }

        public string SetLinks(string currPage, string sectionName, bool fromReview, string selectChoice)
        {
            var sections = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<SectionInfo>> ("NavList");
            int index = sections.FindIndex(x => x.Name == sectionName);
            var section = sections.Find(x => x.Name == sectionName);
            int pageIndex =section!.pages.IndexOf(currPage);
            BackLink = GetBackLink(currPage, fromReview);
            if ((sections.Count == index + 1 || fromReview) && selectChoice == FormSelection.No.ToString()) 
            {
                NextLink = "/" + section.pages[pageIndex + 1];
            }
            else if (sections.Count == index +1 || fromReview)
            {
                //Mobile section always appears last for all users
                NextLink = "/review-page";
            }
            //Should go to edit page
            else if  ((sections.Count != index +1 ) && selectChoice == FormSelection.No.ToString())
            {
                NextLink = "/" + section.pages[pageIndex + 1];
            }
            else
            {
                //follow work flow
               
                    //go to next section first page
                    NextLink = "/" + sections[index + 1].pages[0];
            } 

            
            NextLink = _routes.Single(s=> s.Value == NextLink).Key;
            return NextLink;
        }
    }
}
