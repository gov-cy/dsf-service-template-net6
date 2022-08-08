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
       public class EmailModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string crbEmail { get; set; }
        [BindProperty]
        public string option1 => (string)TempData[nameof(option1)];
        [BindProperty]
        public string option2 => (string)TempData[nameof(option2)];
        [BindProperty]
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string EmailSelection { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";
        [BindProperty]
        public string NextLink { get; set; } = "";
        //Dependancy injection Variables
        public IMyHttpClient _client;
        private IConfiguration _configuration;
        private IValidator<EmailSelect> _validator;
        //Object for session data 
        public EmailSelect Email_select;
        #endregion
        #region "Custom Methods"
        public EmailModel(IValidator<EmailSelect> validator, IMyHttpClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
            _validator = validator;
            Email_select = new EmailSelect();
        }
        //Use to show error messages if web form has errors
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
            if (choice == "Yes")
            {
                NextLink = "/ReviewPage";
            }
            else if (choice == "No")
            {
                NextLink = "/EmailEdit";
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
                res.AddToModelState(this.ModelState, "Email_select");
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
            DisplaySummary = "display:none";
            EmailSelection = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#crbEmail'>" + Item.ErrorMessage + "</a>";
                    EmailSelection = Item.ErrorMessage;
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
            if ((HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime) == null) && (HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime) == null))
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
            SetLinks("EmailSelection", review, "No");
            BackLink = GetBackLink("/" + "EmailSelection");
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
              return  RedirectToAction("LogOut", "Account");
            }
            //Get  Citize details loaded
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            //If coming fromPost
            if (!ShowErrors())
            {
                //Set Email info to model class
                if (string.IsNullOrEmpty(res.data?.email))
                {

                    Email_select.email = User.Claims.First(c => c.Type == "email").Value;
                }
                else
                {
                    Email_select.email = res.data.email;
                }
            }
           
            
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", authTime);
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
            if (crbEmail == "1")
            {
                Email_select.use_from_civil = true;
                Email_select.use_other = false;
            }
            else if (crbEmail == "2")
            {
                Email_select.use_from_civil = false;
                Email_select.use_other = true;
            }
            else
            {
                Email_select.use_from_civil = false;
                Email_select.use_other = false;
            }
            //Re-assign defult email
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (string.IsNullOrEmpty(citizen_data.data?.email))
            {
                Email_select.email = User.Claims.First(c => c.Type == "email").Value;
            }
            else
            {
                Email_select.email = citizen_data.data.email;
            }
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Email_select);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                return RedirectToPage("Email");
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("EmailSelect");
            HttpContext.Session.SetObjectAsJson("EmailSelect", Email_select, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect
            //Set the Back and Next Link
            
            //Set back and Next Link
            
            if (Email_select.use_other)
            {
                SetLinks("EmailSelection", review, "No");
            }
            else
            {
                SetLinks("EmailSelection", review, "Yes");
            }

            return RedirectToPage(NextLink, null, "mainContainer");
        }
    }
}

