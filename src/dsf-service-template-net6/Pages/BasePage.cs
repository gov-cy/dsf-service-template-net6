using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http.Extensions;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Data.Models;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class BasePage : PageModel
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
        public void SetLinks(string curr,bool Review, string choice="0")
        {
            //First add current page to History
             AddHistoryLinks("/" + curr);
            //Set backLink
            BackLink =  GetBackLink("/" + curr);
            //Get Citizen data from Session
            var authTime=string.Empty;
            try
            {
                authTime =  User.Claims.First(c => c.Type == "auth_time").Value ?? "";
            }
            catch{
                authTime = "";
            }
            var citizen_data= new CitizenDataResponse();
            if (!string.IsNullOrEmpty(authTime))
            {
               citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            }
           
            switch (curr)
            {
                case "AddressSelection":
                    if (Review)
                    {  //For Selection Pages only Yes and No exists
                        if (choice == FormSelection.Yes.ToString())
                        {
                            NextLink = "/ReviewPage";
                            
                        }
                        else if (choice == FormSelection.No.ToString())
                        {
                           
                            NextLink = "/AddressEdit/true";
                        }
                       
                    }
                    else
                    {
                        
                        if (choice == FormSelection.Yes.ToString())
                        {
                            if (string.IsNullOrEmpty(citizen_data?.data.mobile))
                            {    
                               NextLink = "/MobileEdit";
                            } else
                            {
                                NextLink = "/Mobile";
                            }
                           
                        }
                        else if (choice == FormSelection.No.ToString())
                        {
                           
                            NextLink = "/AddressEdit";
                        }
                       
                    }
                    break;
                case "SetAddress":
                    //Edit Address will always come from
                    //Selection Page
                    if (Review)
                    {
                        NextLink = "/ReviewPage";
                       
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(citizen_data?.data.mobile))
                        {
                            NextLink= "/MobileEdit";
                        }
                        else
                        {
                            NextLink= "/Mobile";
                        }
                       
                    }
                    break;
                case "MobileSelection":
                    if (Review)
                    {  //For Selection Pages only Yes and No exists
                        if (choice == FormSelection.Yes.ToString())
                        {
                            NextLink = "/ReviewPage";
                        }
                        else if (choice == FormSelection.No.ToString())
                        {
                            NextLink = "/MobileEdit/true";
                        }
                       
                    }
                    else
                    {
                        //Find the back link
                        if (choice == FormSelection.Yes.ToString())
                        {                          
                                NextLink = "/Email";
                                                  
                        }
                        else if (choice == FormSelection.No.ToString())
                        {                            
                            NextLink = "/MobileEdit";
                        }
                      
                    }
                    break;
                case "SetMobile":
                    if (Review)
                    {
                        NextLink = "/ReviewPage";

                    }
                    else
                    {//user always have email
                        NextLink = "/Email";
                    }
                    break;
                case "EmailSelection":
                    //For Selection Pages only Yes and No exists
                        if (choice == FormSelection.Yes.ToString())
                        {
                            NextLink = "/ReviewPage";
                        }
                        else if (choice == FormSelection.No.ToString())
                        {
                            NextLink = "/EmailEdit";
                        }
                     break;
                case "SetEmail":
                    NextLink = "/ReviewPage";
                    break;

                default:
                     break;   
                
            }
        }
        private void AddHistoryLinks(string curr)
        {
            History= HttpContext.Session.GetObjectFromJson<List<string>>("History")?? new List<string>();
            if (History.Count ==0)
            {
                History.Add("/");
            }
            int LastIndex=History.Count-1;
            if (History[LastIndex] !=curr )
            {
                //Add to History
                History.Add(curr);
                //Set to memory
                HttpContext.Session.SetObjectAsJson("History",History);
            }
        }
        private string GetBackLink(string curr)
        {
            History= HttpContext.Session.GetObjectFromJson<List<string>>("History");
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
                return History[currentIndex-1].ToString();
            }
        }
    }
}
