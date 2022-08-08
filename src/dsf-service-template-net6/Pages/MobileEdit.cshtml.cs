using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dsf_service_template_net6.Pages
{
       public class MobileEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private IValidator<MobileEdit> _validator;
        IStringLocalizer _Loc;
        //control variables
        [BindProperty]
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string MobileErrorClass { get; set; } = "";
        [BindProperty]
        public string mobile { get; set; }
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public MobileEdit mobEdit { get; set; }
       
        #endregion
        #region "Custom Methods"
        public MobileEditModel(IValidator<MobileEdit> validator, IStringLocalizer<cMobileEditValidator> Loc)
        {  _validator = validator;
            _Loc = Loc;
            mobEdit = new MobileEdit();
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

            if (Review)
            {
                NextLink = "/ReviewPage";

            }
            else
            {//user always have email
                NextLink = "/Email";
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
        void ClearErrors()
        {
            displaySummary = "display:none";
            MobileErrorClass = "";
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
                res.AddToModelState(this.ModelState, "mobEdit");
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
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "mobile")
                {
                    ErrorsDesc += "<a href='#mobile'>" + Item.ErrorMessage + "</a>";
                    MobileErrorClass = Item.ErrorMessage;
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
            SetLinks("SetMobile", review);
            BackLink = GetBackLink("/" + "SetMobile");
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            //If coming fromPost
            if (!ShowErrors())
            {
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
                if (SessionMobEdit != null)
                {
                    mobEdit = SessionMobEdit;
                }
              
            }
            else
            {
                mobile = HttpContext.Session.GetObjectFromJson<string>("mobileval") ?? "";
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            //Get Previous mobile number
            var citizenPersonalDetails = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (citizenPersonalDetails != null)
            {
                mobEdit.prev_mobile = citizenPersonalDetails.data.mobile;

            }
            // Update the class before validation
            mobEdit.mobile = mobile;
            FluentValidation.Results.ValidationResult result = _validator.Validate(mobEdit);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("mobileval", mobile);
                return RedirectToPage("MobileEdit");
            }
            //Mob Edit from Session
           
            var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            var SessionMobEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime);
            if (SessionMobEdit != null)
            {
                SessionMobEdit.mobile = mobEdit.mobile;
                SessionMobEdit.prev_mobile = mobEdit.prev_mobile;

                HttpContext.Session.Remove("MobEdit");
                HttpContext.Session.SetObjectAsJson("MobEdit", SessionMobEdit, authTime);
            }
            else
            {
                HttpContext.Session.SetObjectAsJson("MobEdit", mobEdit, authTime);
            }
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("mobileval");
            //Finally redirect
            Navigation _nav = new Navigation();
            //Set back and Next Link
            SetLinks("SetMobile", review);
            return RedirectToPage(NextLink, null, "mainContainer");
        }
    }
}
