using DSF.AspNetCore.Web.Template.Data.Models;
using DSF.AspNetCore.Web.Template.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DSF.AspNetCore.Web.Template.Services;
using DSF.AspNetCore.Web.Template.Extensions;
using Microsoft.AspNetCore.Http.Extensions;

namespace DSF.AspNetCore.Web.Template.Pages
{
    public class MobileEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IUserSession _userSession;
        private readonly IValidator<MobileSection> _validator;
        //control variables
        [BindProperty]
        public string DisplaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string MobileErrorClass { get; set; } = "";
        [BindProperty]
        public string Mobile { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Object for session data 
        public MobileSection MobEdit { get; set; }

        #endregion
        #region "Custom Methods"
        public MobileEditModel(IValidator<MobileSection> validator, INavigation nav, IUserSession userSession)
        {
            _validator = validator;
            _userSession = userSession;
            MobEdit = new MobileSection();
            _nav = nav;
        }

        void ClearErrors()
        {
            DisplaySummary = "display:none";
            MobileErrorClass = "";
            ErrorsDesc = "";
        }

        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

            {
                var res = _userSession.GetUserValidationResults()!;
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

        private void SetViewErrorMessages(ValidationResult result)
        {
            if (result != null)
            {
                //First Enable Summary Display
                DisplaySummary = "display:block";
                //Then Build Summary Error
                foreach (ValidationFailure Item in result.Errors)
                {
                    if (Item.PropertyName == nameof(MobEdit.Mobile))
                    {
                        ErrorsDesc += "<a href='#mobile'>" + Item.ErrorMessage + "</a>";
                        MobileErrorClass = Item.ErrorMessage;
                    }
                }
            }

        }

        private bool AllowToProceed()
        {
            bool ret = true;
            //For the demo purpose we might not get data from template api service
            //if (_userSession.GetUserPersonalData() == null)
            //{
            //    ret = false;
            //}
            if (_userSession.GetUserEmailData() == null)
            {
                ret = false;
            }
            return ret;
        }

        private MobileSection? GetSessionData()
        {
            return _userSession.GetUserMobileData();
        }

        private string GetTempSessionData()
        {
            return HttpContext.Session.GetObjectFromJson<string>("mobileval") ?? string.Empty;
        }

        private bool BindData()
        {   //Check if already selected 
            var sessionData = GetSessionData();
            if (sessionData?.ValidationMode == ValidationMode.Edit && sessionData?.UseOther == true)
            {
                Mobile = sessionData.Mobile.FormatMobile();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string SetMobile(string mobile)
        {
            if (!string.IsNullOrEmpty(mobile))
            {
                string formatMob = mobile;
                //if user input= 0035799876543 
                //Remove - and spaces
                formatMob = formatMob.Replace("-", "");
                formatMob = formatMob.Replace(" ", "");
                //Replace + with 00
                formatMob = formatMob.Trim().StartsWith("+") ? $"00{formatMob.Substring(1)}" : formatMob;
                //Add 00357 if cyprus
                formatMob = formatMob.StartsWith("009") ? $"003579{formatMob.Substring(3)}" : formatMob;
                // or format number with 8 digits that starts with 9
                formatMob = formatMob.StartsWith("9") && formatMob.Length == 8 ? $"00357{formatMob}" : formatMob;
                return formatMob;
            }
            else
            {
                return "";
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
            BackLink = _nav.GetBackLink("/set-mobile", review);
            //If coming fromPost
            if (!fromPost)
            { //GetData from session 
                BindData();
            }
            else
            {
                Mobile = GetTempSessionData();
                ShowErrors(true);
            }

            return Page();
        }
        public IActionResult OnPost(bool review)
        { // Update the class before validation
            string typemob = Mobile;
            Mobile = SetMobile(Mobile);
            MobEdit.Mobile = Mobile;
            MobEdit.UseOther = true;
            MobEdit.UseFromApi = false;
            MobEdit.ValidationMode = ValidationMode.Edit;
            FluentValidation.Results.ValidationResult result = _validator.Validate(MobEdit);
            if (!result.IsValid)
            {
                _userSession.SetUserValidationResults(result);
                HttpContext.Session.SetObjectAsJson("mobileval", typemob);
                return RedirectToPage("MobileEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Mob Edit from Session
            _userSession.SetUserMobileData(MobEdit);

            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("mobileval");

            //Set back and Next Link
            NextLink = _nav.SetLinks("set-mobile", "Mobile", review, "NoSelection");
            //clear # in  URLs  that generates on error found
            bool hashInUrl = HttpContext.Request.GetDisplayUrl().Contains("fromPost");
            return RedirectToPage(NextLink, null, null, !hashInUrl ? null : "");
        }
    }
}
