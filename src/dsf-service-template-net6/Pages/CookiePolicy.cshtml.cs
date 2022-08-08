using dsf_service_template_net6.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dsf_service_template_net6.Pages
{
    public class CookiePolicyModel : PageModel
    {
        [BindProperty]
        public string BackLink { get; set; } = "";
               
        public void OnGet()
        {
            Navigation _nav = new Navigation();
            //Set back and Next Link
            AddHistoryLinks("CookiePolicy");
            BackLink = GetBackLink("/" + "CookiePolicy");
        }
        private string GetBackLink(string curr)
        {
            var History = HttpContext.Session.GetObjectFromJson<List<string>>("History");
            int currentIndex = History.FindIndex(x => x == curr);
            //if not found
            if (currentIndex == -1)
            {
                return "/";
            }
            //Last value in history
            else if (currentIndex == 0)
            {
                var index = History.Count - 1;
                return History[index].ToString();
            }
            //Return the previus of current
            else
            {
                return History[currentIndex - 1].ToString();
            }
        }
        public void AddHistoryLinks(string curr)
        {

            var History = HttpContext?.Session.GetObjectFromJson<List<string>>("History") ?? new List<string>();
            if (History.Count == 0)
            {
                History.Add("/");
            }
            int LastIndex = History.Count - 1;
            if (History[LastIndex] != curr)
            {
                //Add to History
                History.Add(curr);
                //Set to memory

                HttpContext.Session.SetObjectAsJson("History", History);
            }
        }
    }
}
