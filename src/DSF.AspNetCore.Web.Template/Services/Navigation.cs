using DSF.AspNetCore.Web.Template.Services.Model;

namespace DSF.AspNetCore.Web.Template.Services
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
        private readonly IContact _contactService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserSession _userSession;
        private readonly Dictionary<string, string> _routes = new()
        {
            { "/Email", "/email-selection" },
            { "/EmailEdit", "/set-email" },
            { "/Mobile", "/mobile-selection" },
            { "/MobileEdit", "/set-mobile" },
            { "/ReviewPage", "/review-page" },
        };
        private List<HistoryItem> History { get; set; } = new();
        public string BackLink { get; set; } = string.Empty;
        public string NextLink { get; set; } = string.Empty;

        private void AddHistoryLinks(string currPage, bool review)
        {
            HistoryItem? historyItem = new();
            History = _userSession.GetHistrory() ?? new List<HistoryItem>();
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
            _userSession.SetHistrory(History);
        }

        private void SetSectionPages()
        {
            ContactInfoResponse? res;
            List<SectionInfo> list = new();
            var citizen = _userSession?.GetUserPersonalData();
            if (citizen == null)
            { //Try to get data from api
                res = _contactService.GetContact(_userSession!.GetAccessToken()!);
                //if the user is already login and not passed from login, set in session
                if (res?.Data != null)
                {
                    _userSession.SetUserPersonalData(res);
                    citizen = res;
                }
            }

            //Add Email Section
            SectionInfo section = new()
            {
                Name = "Email",
                SectionOrder = 1,

                //Always Select, for even API does not have email, we show email from user profile
                //The email on user Ariadne profile is always considered verified
                Type = SectionType.SelectionAndInput
            };
            section.pages.Add("email-selection");
            section.pages.Add("set-email");
            list.Add(section);

            //Add Mobile section,
            section = new()
            {
                Name = "Mobile",
                SectionOrder = 2,

                //
                Type = (!string.IsNullOrEmpty(citizen?.Data?.MobileTelephone)) ? SectionType.SelectionAndInput : SectionType.InputOnly
            };
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
            _contactService = service;
            _userSession = userSession;
            var ListItem = _userSession.GetNavLink().FirstOrDefault();
            if (ListItem == null)
            {
                BackLink = "/";
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    SetSectionPages();
                    ListItem = _userSession!.GetNavLink()!.First();
                    NextLink = "/" + ListItem.pages.First();
                }
            }
        }

        public string GetBackLink(string currPage, bool fromReview = false)
        {
            History = _userSession!.GetHistrory();
            if (fromReview || History.Count==0)
            {
                AddHistoryLinks(currPage, fromReview);
            } 
            HistoryItem? Item = History.Find(x => x.PageName == currPage && x.Review == fromReview);
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
                    PrevItem = History.Count > 1 ? History[History.Count - 1] : History[0];
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
                int index = History.FindIndex(x =>  x.Review==fromReview && x.PageName==currPage );
                //set the prev item 
                Item = History[index - 1];
                Item.PageName = (Item.Review ? Item.PageName + "?review=true" : Item.PageName);
                BackLink = Item.PageName;
                return Item.PageName;

            }

        }

        public string SetLinks(string currPage, string sectionName, bool fromReview, string selectChoice)
        {
            //First add current page to History

            AddHistoryLinks("/" + currPage, fromReview);
            
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
