using dsf_service_template_net6.Extensions;
using Microsoft.AspNetCore.Mvc; 

namespace dsf_service_template_net6.Controllers
{
    public class AddToHistoryController : Controller
{
        

        public IActionResult Index()
    {
       
        return View();
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
