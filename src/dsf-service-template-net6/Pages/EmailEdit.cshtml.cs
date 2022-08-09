using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Pages
{
    
    public class EmailEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly IValidator<EmailEdit> _validator;
        [BindProperty]
        //control variables
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string EmailErrorClass { get; set; } = "";
        [BindProperty]
        public string EmailSelection { get; set; } = "";
        [BindProperty]
        public string email { get; set; }
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public EmailEdit emailEdit;
        Navigation _nav = new Navigation();
        #endregion
        #region "Custom Methods"
        public EmailEditModel(IValidator<EmailEdit> validator)
        {   _validator = validator;
             emailEdit = new EmailEdit();
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

            NextLink = "/ReviewPage";
        }
        private string GetBackLink(string curr)
        {
            var History = HttpContext.Session.GetObjectFromJson<List<string>>("History");
            int currentIndex = History.FindLastIndex(x => x == curr);
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
        void ClearErrors()
        {
            DisplaySummary = "display:none";
            EmailErrorClass = "";
            ErrorsDesc = "";
        }
        bool ShowErrors()
        {
            if (HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult") != null)

            {
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "emailEdit");
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
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            DisplaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "email")
                {
                    ErrorsDesc += "<a href='#email'>" + Item.ErrorMessage + "</a>";
                    EmailErrorClass = Item.ErrorMessage;
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
           SetLinks("SetEmail", review);
           BackLink = GetBackLink("/" + "SetEmail");
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            //If coming fromPost
            if (!ShowErrors())
            {
                //GetData from session 
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", authTime);
                if (SessionEmailEdit != null)
                {
                    email = SessionEmailEdit.email;
                }
               
            }
            else
            {
                email = HttpContext.Session.GetObjectFromJson<string>("emailval") ;
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Update the class before validation
            emailEdit.email = email;
            //Get Previous mobile number
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                emailEdit.prev_email = citizenPersonalDetails.data.email ??  User.Claims.First(c => c.Type == "email").Value;

            }
            FluentValidation.Results.ValidationResult result = _validator.Validate(emailEdit);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("emailval", email);
                return RedirectToPage("EmailEdit");
            }
            //Store Data 
            HttpContext.Session.Remove("EmailEdit");
            HttpContext.Session.SetObjectAsJson("EmailEdit", emailEdit, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("emailval");
            //Finall redirect
            Navigation _nav = new Navigation();
            //Set back and Next Link
            SetLinks("SetEmail", review);
           
                return RedirectToPage(NextLink, null, "mainContainer");
            
        }
    }
   
}
