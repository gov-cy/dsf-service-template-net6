using dsf_service_template_net6.Services.Model;
using System.Security.Claims;
namespace dsf_service_template_net6.Services
{
    using Microsoft.AspNetCore.Http;
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
        private readonly IContact _service;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly IUserSession? _userSession;
        public string BackLink { get; set; } = "";
        public string NextLink { get; set; } = "";
        private List<HistoryItem> History { get; set; } = new List<HistoryItem>();
        readonly Dictionary<string, string> _routes = new()
        {
            { "/Email", "/email-selection" },
            { "/EmailEdit", "/set-email" },
            { "/Mobile", "/mobile-selection" },
            { "/MobileEdit", "/set-mobile" },
            { "/ReviewPage", "/review-page" },
        };

        private void AddHistoryLinks(string currPage, bool review)
        {
            HistoryItem? historyItem = new();
            History = _userSession?.GetHistrory() ?? new List<HistoryItem>();
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
            _userSession!.SetHistrory(History);
        }
        private void SetSectionPages()
        {
            ContactInfoResponse? res;
            List<SectionInfo> list = new();
            var citizen = _userSession?.GetUserPersonalData();
            if (citizen == null)
            { //Try to get data from api
                res = _service.GetContact(_userSession!.GetAccessToken()!);
                //if the user is already login and not passed from login, set in session
                if (res?.Data != null)
                {
                    _userSession.SetUserPersonalData(res);
                    citizen = _userSession.GetUserPersonalData();
                }

               
            }
            //New Section
            SectionInfo section = new();
            section.Name = "Email";
            section.SectionOrder = 1;
            //Always Select, for even API does not have email, we show email from user profile 
            section.Type = SectionType.SelectionAndInput;
            section.pages.Add("email-selection");
            section.pages.Add("set-email");
            list.Add(section);
            //New Section
            section = new();
            section.Name = "Mobile";
            section.SectionOrder = 2;
            //Always Select, for even API does not have email, we show email from user profile 
            section.Type = (!string.IsNullOrEmpty(citizen?.Data?.MobileTelephone)) ? SectionType.SelectionAndInput : SectionType.InputOnly;
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
            _userSession!.SetNavLink(list);
        }
        public Navigation(IHttpContextAccessor httpContextAccessor, IContact service, IUserSession userSession)
        {
            _httpContextAccessor = httpContextAccessor;
            _service = service;
            _userSession = userSession;
            var ListItem = _userSession?.GetNavLink()?.First();
            if (ListItem == null)
            {
                BackLink = "/";
                if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    SetSectionPages();
                    ListItem = _userSession!.GetNavLink()!.First();
                    NextLink = "/" + ListItem.pages.First();
                }

            }

        }

        public string GetBackLink(string currPage, bool fromReview = false)
        {
            History = _userSession!.GetHistrory()!;

            AddHistoryLinks(currPage, fromReview);

            HistoryItem? Item = History.FindLast(x => x.PageName == currPage && x.Review == fromReview);
            HistoryItem? PrevItem = null;
            //if not found get the previous
            if (Item == null)
            {
                bool Found = History.Exists(x => x.PageName == currPage);
                if (Found)
                {
                    PrevItem = (History.Count > 1 ? History[History!.Count - 2] : History[History.Count - 1]);
                }
                else
                {
                    //Just show Previous
                    PrevItem = (History.Count > 1 ? History[History.Count - 1] : History[0]);
                }
                if (PrevItem == null)
                {
                    Item = new HistoryItem
                    {
                        PageName = "/"
                    };
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
                int index = History.FindLastIndex(x =>  x.Review==fromReview && x.PageName==currPage );
                //set the prev item 
                Item = History[index - 1];
                Item.PageName = (Item.Review ? Item.PageName + "?review=true" : Item.PageName);
                BackLink = Item.PageName;
                return Item.PageName;

            }

        }

        public string SetLinks(string currPage, string sectionName, bool fromReview, string selectChoice)
        {
            var sections = _userSession!.GetNavLink();
            int index = sections!.FindIndex(x => x.Name == sectionName);
            var section = sections.Find(x => x.Name == sectionName);
            int pageIndex = section!.pages.IndexOf(currPage);
            BackLink = GetBackLink("/" + currPage, fromReview);
            if ((sections.Count == index + 1) && selectChoice == FormSelection.No.ToString())
            {
                NextLink = "/" + section.pages[pageIndex + 1];
            }
            else if (sections.Count == index + 1 && (selectChoice == FormSelection.Yes.ToString() || selectChoice == FormSelection.NoSelection.ToString()))
            {
                //Mobile section always appears last for all users
                NextLink = "/review-page";
            }
            //Should go to edit page
            else if ((sections.Count != index + 1) && selectChoice == FormSelection.No.ToString())
            {
                NextLink = "/" + section.pages[pageIndex + 1];
            }
            else
            {
                //follow work flow
                if (fromReview)
                {
                    NextLink = "/review-page";
                }
                else
                {
                    //go to next section first page
                    NextLink = "/" + sections[index + 1].pages[0];
                }

            }


            NextLink = _routes.Single(s => s.Value == NextLink).Key;
            return NextLink;
        }
    }
}
