using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Services.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class MobileModel : PageModel
    {
        #region "Variables"
        //control variables
        public string crbMobile { get; set; } = "";
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string MobileSelection = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";

        //Dependancy injection Variables
        private readonly INavigation _nav;
        private IValidator<MobileSection> _validator;
        //Object for session data 
        public MobileSection Mobile_select;
        #endregion
        #region "Custom Methods"
        public MobileModel(IValidator<MobileSection> validator, INavigation nav)
        {
            _nav = nav;
            _validator = validator;
            Mobile_select = new MobileSection();
        }
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

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
            if (GetCitizenDataFromApi == null)
            {
                ret = false;
            }
            if ((HttpContext.Session.GetObjectFromJson<EmailSection>("EmailSection", GetAuthTime()) == null))
            {
                ret = false;
            }
            return ret;
        }
        private ContactInfoResponse GetCitizenDataFromApi()
        {
            ContactInfoResponse res = HttpContext.Session.GetObjectFromJson<ContactInfoResponse>("PersonalDetails", GetAuthTime());
            return res;
        }
        private string GetAuthTime()
        {
            return User.Claims.First(c => c.Type == "auth_time").Value;
        }
       
        private MobileSection GetSessionData()
        {
            var selectedoptions = HttpContext.Session.GetObjectFromJson<MobileSection>("MobileSection", GetAuthTime());
            return selectedoptions;
        }
        private void BindSelectionData()
        {
            ContactInfoResponse res = GetCitizenDataFromApi();
            //Set Email info to model class
            if (!string.IsNullOrEmpty(res.data?.mobileTelephone))
            {

                Mobile_select.mobile = res.data.mobileTelephone;
            }
            
        }
        private bool BindData()
        {   //Check if already selected 
            var selectedoptions = GetSessionData();
            if (selectedoptions != null)
            {
                if (selectedoptions.use_from_api)
                {
                    crbMobile = "1";
                }
                else if (selectedoptions.use_other && selectedoptions.mobile== GetCitizenDataFromApi()?.data?.mobileTelephone)
                {
                    //code use when user hit back button on edit page
                    crbMobile = "1";
                    Mobile_select.use_from_api = true;
                    Mobile_select.use_other = false;
                    Mobile_select.mobile = GetCitizenDataFromApi().data.mobileTelephone;
                    HttpContext.Session.SetObjectAsJson("MobileSelect", Mobile_select, GetAuthTime());
                }
                else
                {
                    crbMobile = "2";
                }
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
            // First set back link
            BackLink = _nav.GetBackLink("/mobile-selection", review);
            //Show selection Data 
            BindSelectionData();
            if (fromPost)
            {
                ShowErrors(true);
            }else
            {
                BindData();
            }
          
            return Page();
        }
        public IActionResult OnPost(bool review)
        {
            //Set class Model before validation
            if (crbMobile == "1")
            {
                Mobile_select.use_from_api = true;
                Mobile_select.use_other = false;
            }
            else if (crbMobile == "2")
            {
                Mobile_select.use_from_api = false;
                Mobile_select.use_other = true;
            }
            else
            {
                Mobile_select.use_from_api = false;
                Mobile_select.use_other = false;
            }
            //Re-assign defult email
            BindSelectionData();
            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(Mobile_select);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                return RedirectToPage("Mobile", null, new { fromPost = true }, "mainContainer");
            }
            //Model is valid so strore 
            HttpContext.Session.Remove("MobileSelect");
            HttpContext.Session.SetObjectAsJson("MobileSelect", Mobile_select, GetAuthTime());
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect
            
            //Set back and Next Link

            if (Mobile_select.use_other)
            {
                NextLink = _nav.SetLinks("mobile-selection","Mobile", review, "No");
            }
            else
            {
                NextLink = _nav.SetLinks("mobile-selection", "Mobile", review, "Yes");
            }
            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review }, "mainContainer");
            }
            else
            {
                return RedirectToPage(NextLink);
            }

        }
    }
}
