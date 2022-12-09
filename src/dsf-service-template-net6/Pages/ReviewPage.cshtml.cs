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
        private readonly IContact _service;
        public ReviewPageModel(INavigation nav, IContact service)
        {
            _nav = nav;
            _service = service;
        }
        #region "Variables"
        ContactInfo _application = new();
        public string currentLanguage = "";
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
                return RedirectToAction("LogOut", "Account");
            }
            return Page();
        }
        public IActionResult OnPostApplicationSubmit()
        {   //Set ApplicationRequest
            var ret = SetApplication();
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

            if (HttpContext.Session.GetObjectFromJson<ContactInfoResponse>("PersonalDetails", GetAuthTime()) == null)
            {
                ret = false;
            }

            if ((HttpContext.Session.GetObjectFromJson<MobileSection>("MobileSection", GetAuthTime()) == null))
            {
                ret = false;
            }

            if ((HttpContext.Session.GetObjectFromJson<EmailSection>("EmailSection", GetAuthTime()) == null))
            {
                ret = false;
            }
            return ret;
        }
        private bool SetUserJourneyData()
        {
            bool ret = true;
            var mobSelect = HttpContext.Session.GetObjectFromJson<MobileSection>("MobileSection", GetAuthTime());
            ret_mobile = mobSelect.mobile;
            var Nav = HttpContext!.Session.GetObjectFromJson<List<SectionInfo>>("NavList");
            var section = Nav.Find(p => p.Name == "Mobile");
            useMobileEditOnly = section!.Type == SectionType.InputOnly ? true : false;
            var emailSelect = HttpContext.Session.GetObjectFromJson<EmailSection>("EmailSection", GetAuthTime());
            ret_email = emailSelect.email;
            section = Nav.Find(p => p.Name == "Email");
            useEmailEditOnly = section!.Type == SectionType.InputOnly ? true : false;
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
                    ContactInfo data = new();
                    data.id = 1;
                    data.email = ret_email;
                    data.mobileTelephone = ret_mobile;
                                       
                }
            }
            return ret;
        }
        private bool SubmitApplication()
        {
            bool ret = false;
            var authTime = GetAuthTime();
            var token = HttpContext.Session.GetObjectFromJson<string>("access_token", authTime);
            ContactInfoResponse? res = new();
            res = _service.SubmitContact(_application, token);
            
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
