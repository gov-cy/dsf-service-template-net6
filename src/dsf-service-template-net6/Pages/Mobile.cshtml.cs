using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class MobileModel : PageModel
    {
        #region "Variables"
        //control variables
        public string crbMobile { get; set; }
        public string option1 => (string)TempData[nameof(option1)];
        public string option2 => (string)TempData[nameof(option2)];
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string MobileSelection = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";

        //Dependancy injection Variables
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<MobileSelect> _validator;
        //Object for session data 
        public MobileSelect Mobile_select;
        Navigation _nav = new Navigation();
        #endregion
        #region "Custom Methods"
        public MobileModel(IValidator<MobileSelect> validator, IMyHttpClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
            _validator = validator;
            Mobile_select = new MobileSelect();
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
        public void SetLinks(string curr, bool Review, string choice = "0")
        {
            //First add current page to History
            AddHistoryLinks("/" + curr);
            //Get Citizen data from Session
                        
            if (Review)
            {  //For Selection Pages only Yes and No exists
                if (choice == "Yes")
                {
                    NextLink = "/ReviewPage";
                }
                else if (choice == "No")
                {
                    NextLink = "/MobileEdit";
                }

            }
            else
            {
                //Find the back link
                if (choice == "Yes")
                {
                    NextLink = "/Email";

                }
                else if (choice == "No")
                {
                    NextLink = "/MobileEdit";
                }

            }

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
        bool ShowErrors()
        {
            if (HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult") != null)

            {
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "Mobile_select");
                //Update Error messages on View
                ClearErrors();
                SetViewErrorMessages(res);
                return true;
            }
            else
            {
                return false;
            }
        }
        void ClearErrors()
        {
            displaySummary = "display:none";
            MobileSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "mobile")
                {
                    ErrorsDesc += "<a href='#crbMobile'>" + Item.ErrorMessage + "</a>";
                    MobileSelection = Item.ErrorMessage;
                }

            }
        }
        private bool AllowToProceed()
        {
            bool ret = true;
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            {
                ret = false;
            }
            if (HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime) == null)
            {
                ret = false;
            }
            return ret;
        }
        #endregion
        public IActionResult OnGet(bool review)
        {
            Navigation _nav = new Navigation();
             //Set back and Next Link
            SetLinks("MobileSelection", review, "No");
            BackLink = GetBackLink("/" + "MobileSelection");
            //Chack if user has sequentialy load the page
            bool allow=AllowToProceed();
            if (!allow)
            {
               return  RedirectToAction("LogOut", "Account");
            }
            //Get  Citize details loaded
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res= HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            //If coming fromPost
            if (!ShowErrors())
            {
                //Set Mobile info to model class
                Mobile_select.mobile = res.data.mobile;
            }
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime);
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_civil)
                {
                    TempData["option1"] = "true";
                    TempData["option2"] = "false";
                }
                else
                {
                    TempData["option1"] = "false";
                    TempData["option2"] = "true";
                }
            }
            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (crbMobile == "1")
            {
                Mobile_select.use_from_civil = true;
                Mobile_select.use_other = false;
            }
            else if (crbMobile == "2")
            {
                Mobile_select.use_from_civil = false;
                Mobile_select.use_other = true;
            }
            else
            {
                Mobile_select.use_from_civil = false;
                Mobile_select.use_other = false;
            }
            //Re-assign defult Mobile
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            Mobile_select.mobile = citizen_data.data.mobile;
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Mobile_select);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                return RedirectToPage("Mobile");
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("MobileSelect");
            HttpContext.Session.SetObjectAsJson("MobileSelect", Mobile_select, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect
            //Set the Back and Next Link
            Navigation _nav = new Navigation();
            //Set back and Next Link
            
            if (Mobile_select.use_other)
            {
                SetLinks("MobileSelection", review, "No");
            }
            else
            {
                SetLinks("MobileSelection", review, "Yes");
            }
            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review }, "mainContainer");
            }
            else
            {
                return RedirectToPage(NextLink, null, "mainContainer");
            }

        }
    }
}
