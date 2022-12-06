using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Services.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class ReviewPageModel : PageModel
    {
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly ITasks _service;
        public ReviewPageModel(INavigation nav, ITasks service)
        {
            _nav = nav;
            _service = service;
        }
        #region "Variables"
        public TasksResponse _citizenPersonalDetails = new TasksResponse();
        public List<dsf_service_template_net6.Services.Model.Task> _application = new();
        public string currentLanguage="";
        //Data retrieve from other pages
        public string ret_email = string.Empty;
        public string ret_mobile = string.Empty;
        public bool useEmailEditOnly = false;
        public bool useMobileEditOnly = false;
        [BindProperty]
        public string BackLink { get; set; } = "";
        #endregion
        private string GetAuthTime()
        {
            return User.Claims.First(c => c.Type == "auth_time").Value;
        }
        public IActionResult OnGet(bool review)
        {   
            bool allow = AllowToProceed();
            if (!allow)
            {
              return RedirectToAction("LogOut", "Account");
            }
            //Get back link
            BackLink = _nav.GetBackLink("/review-page", true);
            //Set Data from journey pages
            bool proceed = SetUserJourneyData();
            if (!proceed)
            {
              return  RedirectToAction("LogOut", "Account");
            }
            return Page();
        }
        public IActionResult OnPostApplicationSubmit()
        {   //Save the Application
            //Set ApplicationRequest
            var  ret = SetApplication();
            if (!ret)
            {
                return RedirectToPage("/ServerError");
            }
            if (SubmitApplication())
                {
                    return RedirectToPage("/ApplicationResponse");
                }
                else
                {
                    return RedirectToPage("/ServerError");
                }
        }
        #region "Custom Methods"
          private bool AllowToProceed()
        {
            //Make sure the sequence has been kept
            bool ret = true;
            
            if (HttpContext.Session.GetObjectFromJson<TasksResponse>("PersonalDetails", GetAuthTime()) == null)
            {
                ret = false;
            }
           
            if ((HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", GetAuthTime()) == null) && (HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", GetAuthTime()) == null))
            {
                ret = false;
            }

            if ((HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", GetAuthTime()) == null) && (HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", GetAuthTime()) == null))
            {
                ret = false;
            }
            return ret;
        }
          private bool SetUserJourneyData()
          {
            bool ret = true;
                     
            //first get Address Select
            _citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<TasksResponse>("PersonalDetails", GetAuthTime());
            
          
            var mobSelect = HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", GetAuthTime());
            if (mobSelect != null)
            {   
                if (mobSelect.use_from_civil)
                {
                ret_mobile = _citizenPersonalDetails.data?.ToList()?.Find(x => x.id == 2)?.name;
                }
                else
                {
                var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", GetAuthTime());
                ret_mobile = SessionMobEdit.mobile;
                }

            } else
            {
                //Directly to edit
                var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", GetAuthTime());
                ret_mobile = SessionMobEdit.mobile;
                useMobileEditOnly = true;
            }
         
            var emailSelect = HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", GetAuthTime());
           if (emailSelect != null)
            {
                if (emailSelect.use_from_civil)
                {
                ret_email= _citizenPersonalDetails.data?.First()?.name ?? User.Claims.First(c => c.Type == "email").Value; 
                } else 
                {
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", GetAuthTime());
                ret_email = SessionEmailEdit.email;
                }
            }else
            {
                //Directrly to email edit
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", GetAuthTime());
                ret_email = SessionEmailEdit.email;
                useEmailEditOnly = true;
            }
           
            
             return ret;
        }
          private bool SetApplication()
        {
            bool ret = true;
            bool isDataRetrieve = SetUserJourneyData();
            if (isDataRetrieve)
            {                
                if (string.IsNullOrEmpty(ret_email) || string.IsNullOrEmpty(ret_mobile))
                {
                    ret = false;
                }
                else
                {
                    Services.Model.Task mobile, email =new();
                    email.id = 1;
                    email.name = ret_email;
                    email.isComplete = true;
                    mobile = new Services.Model.Task(); 
                    mobile.id = 2;
                    mobile.name = ret_mobile;
                    mobile.isComplete = true;
                    _application.Add(email);
                    _application.Add(mobile); 
                }
          }
            return ret;
        }
          private bool SubmitApplication()
        {
            bool ret = false;
            var authTime = GetAuthTime();
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            TasksResponse? res=new();
            foreach (Services.Model.Task item in _application)
            {
                res = _service.SubmitTask(item, token);
            } 
            if (res.succeeded)
            {
                //Redirect if error code is <> 0
                if (res.errorCode == 0)
                {
                    ret = true;
                    HttpContext.Session.SetObjectAsJson("ApplReq", _application, authTime);
                    HttpContext.Session.SetObjectAsJson("ref_no", Guid.NewGuid(), authTime);
                }
                else
                {
                    //Log response error
                    HttpContext.Session.SetObjectAsJson("ApplRes", res, authTime);
                }
            }
            else
            {
                //Log response error
                HttpContext.Session.SetObjectAsJson("ApplRes", res, authTime);
            }
            return ret;
        }
        #endregion

    }
}
