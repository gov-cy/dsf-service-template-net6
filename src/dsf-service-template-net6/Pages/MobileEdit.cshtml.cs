using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using dsf_service_template_net6.Services;

namespace dsf_service_template_net6.Pages
{
       public class MobileEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private IValidator<MobileEdit> _validator;
         //control variables
        [BindProperty]
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string MobileErrorClass { get; set; } = "";
        [BindProperty]
        public string mobile { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public MobileEdit mobEdit { get; set; }
       
        #endregion
        #region "Custom Methods"
        public MobileEditModel(IValidator<MobileEdit> validator, INavigation nav)
        {  _validator = validator;
            mobEdit = new MobileEdit();
            _nav = nav;
        }
     
        void ClearErrors()
        {
            displaySummary = "display:none";
            MobileErrorClass = "";
            ErrorsDesc = "";
        }
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

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
            if (GetCitizenDataFromApi == null)
            {
                ret = false;
            }
            if ((HttpContext.Session.GetObjectFromJson<EmailSelect>("EmailSelect", GetAuthTime()) == null) && (HttpContext.Session.GetObjectFromJson<EmailEdit>("EmailEdit", GetAuthTime()) == null))
            {
                ret = false;
            }
            return ret;
        }
        private string GetAuthTime()
        {
            return User.Claims.First(c => c.Type == "auth_time").Value;
        }
        private CitizenDataResponse GetCitizenDataFromApi()
        {
            CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", GetAuthTime());
            return res;
        }
        private MobileEdit GetSessionData()
        {
            var SessionEmailEdit = HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", GetAuthTime());
            return SessionEmailEdit;
        }
        private string GetTempSessionData()
        {
            var tempSession = HttpContext.Session.GetObjectFromJson<string>("mobileval");
            return tempSession;
        }
        private bool BindData()
        {   //Check if already selected 
            var sessionData = GetSessionData();
            if (sessionData != null)
            {
                mobile = sessionData.mobile;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        public IActionResult OnGet(bool review, bool fromPost)
        {
            //Chack if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            //Set Back Link
            BackLink = _nav.GetBackLink("/set-email", review);
            //If coming fromPost
            if (!fromPost)
            { //GetData from session 
                BindData();
            }
            else
            {
                mobile = GetTempSessionData();
                ShowErrors(true);
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        { // Update the class before validation
            mobEdit.mobile = mobile;
           //Get Previous mobile number
            var citizenPersonalDetails = GetCitizenDataFromApi();
            if (citizenPersonalDetails != null)
            {
                mobEdit.prev_mobile = citizenPersonalDetails.data.mobile;

            }
           
            FluentValidation.Results.ValidationResult result = _validator.Validate(mobEdit);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("mobileval", mobile);
                return RedirectToPage("MobileEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Mob Edit from Session
            HttpContext.Session.Remove("MobEdit");
            HttpContext.Session.SetObjectAsJson("MobEdit", mobEdit, GetAuthTime());

            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("mobileval");
    
            //Set back and Next Link
            _nav.SetLinks("set-mobile", "Mobile", review, "NoSelection");
            return RedirectToPage(NextLink);
        }
    }
}
