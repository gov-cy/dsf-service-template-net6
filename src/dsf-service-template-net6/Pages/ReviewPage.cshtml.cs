using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Extensions;
using Dsf.Service.Template.Services;
using Dsf.Service.Template.Services.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Dsf.Service.Template.Pages
{
    [BindProperties]
    public class ReviewPageModel : PageModel
    {
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IContact _service;
        private readonly IUserSession _userSession;
        public ReviewPageModel(INavigation nav, IContact service, IUserSession userSession)
        {
            _nav = nav;
            _service = service;
            _userSession = userSession;
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

            //For the demo purpose we might not get data from template api service
            //if (_userSession.GetUserPersonalData() == null)
            //{
            //    ret = false;
            //}

            if (_userSession.GetUserMobileData() == null)
            {
                ret = false;
            }

            if (_userSession.GetUserEmailData() == null)
            {
                ret = false;
            }
            return ret;
        }
        private bool SetUserJourneyData()
        {
            bool ret = true;
            var mobSelect = _userSession!.GetUserMobileData()!;
            ret_mobile = mobSelect.mobile;
            var Nav = _userSession.GetNavLink()!;
            var section = Nav.Find(p => p.Name == "Mobile");
            useMobileEditOnly = section!.Type == SectionType.InputOnly ? true : false;
            var emailSelect = _userSession.GetUserEmailData()!;
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
                    _application.Id = 1;
                    _application.Email = ret_email;
                    _application.MobileTelephone = ret_mobile;
                                       
                }
            }
            return ret;
        }
        private bool SubmitApplication()
        {
            bool ret = false;
            var token = _userSession.GetAccessToken()!;
            ContactInfoResponse? res = new();
            res = _service.SubmitContact(_application, token);
            
            if (res.Succeeded)
            {
                //Redirect if error code is <> 0
                if (res.ErrorCode == 0)
                {
                    ret = true;
                    _userSession.SetUserApplRequest(_application);
                    _userSession.SetUserReferenceNumber(Guid.NewGuid().ToString());
                }
                else
                {
                    //Save Invalid Response in session
                    _userSession.SetUserApplResponse(res);
                }
            }
            else
            {
                //Save Invalid Response in session
                _userSession.SetUserApplResponse(res);
            }
            return ret;
        }
        #endregion

    }
}
