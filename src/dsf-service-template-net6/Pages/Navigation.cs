using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http.Extensions;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Data.Models;

namespace dsf_service_template_net6.Pages
{
   
    public class Navigation : PageModel
    {        
        public string BackLink { get; set; } = "";
        public string NextLink { get; set; } = "";
        private List<string> History { get; set; }=new List<string>();
        public enum FormSelection
        {         
            Yes,
            No,
            NoSelection
        }
        //Constructor
        
        public Navigation()
        {
            History = new List<string>();
        }
       
        public void AddHistoryLinks(string curr)
        {

            RedirectToAction("AddHistoryLinks", "AddToHistory", new { curr = curr });
        }
     
    }
}
