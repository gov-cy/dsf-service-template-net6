using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Pages.Shared;
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
    public class AddressModel : PageModel
    {
        #region "Variables"
        //control variables
        [BindProperty]
        public string crbAddress { get; set; } = String.Empty;
        [BindProperty]
        public string option1 => (string)TempData[nameof(option1)];
        [BindProperty]
        public string option2 => (string)TempData[nameof(option2)];
        [BindProperty]
        public string displaySummary { get; set; } = "display:none";
        [BindProperty]
        public string ErrorsDesc { get; set; } = "";
        [BindProperty]
        public string AddSellectError { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";

        [BindProperty]
        public string NextLink { get; set; } = "";
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IMoiCrmd _service;
        private IValidator<AddressSelect> _validator;
        //Object for session data, will be set internally
        //from web form variables
        public AddressSelect address_select;
        #endregion
        #region "Custom Methods"
        //Constructor
        public AddressModel(IValidator<AddressSelect> validator, IMoiCrmd service, INavigation nav)
        {
            _service = service;
            _validator = validator;
            address_select = new AddressSelect();
            _nav = nav;
        }

        //Use to show error messages if web form has errors
        bool ShowErrors(bool fromPost)
        {
            if (fromPost)

            {
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(this.ModelState, "address_select");
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
            AddSellectError = "";
            ErrorsDesc = "";
        }
        private void SetViewErrorMessages(FluentValidation.Results.ValidationResult result)
        {
            //First Enable Summary Display
            displaySummary = "display:block";
            //Then Build Summary Error
            foreach (ValidationFailure Item in result.Errors)
            {
                if (Item.PropertyName == "use_from_civil" || Item.PropertyName == "addressInfo")
                {
                    ErrorsDesc += "<a href='#crbAddress'>" + Item.ErrorMessage + "</a>";
                    AddSellectError = Item.ErrorMessage;
                }

            }
        }
        private bool BindData()
        {
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            //Check if already selected 
            var selectedoptions = HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime);
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
            //First set back link
            BackLink = _nav.GetBackLink("/address-selection", review);
            if (fromPost)
            {
                //I just need to bind address info 
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
                address_select.addressInfo = res.data.addressInfo;
                ShowErrors(true);

            }
            else
            {
                //check for revisit
                bool revisit = BindData();
                if (!revisit)
                {
                    //Check whether api data were retrieve from login , otherwise call again
                    var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                    CitizenDataResponse res = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
                    if (res == null)
                    {

                        var lang = "";
                        if (Thread.CurrentThread.CurrentUICulture.Name == "el-GR")
                        {
                            lang = "el";
                        }
                        else
                        {
                            lang = "en";
                        }

                        //Call the citizen personal details from civil registry  
                        res = _service.GetCitizenData(lang, "");
                        if (res.succeeded == false)
                        {
                            return RedirectToPage("/ServerError");
                        }
                        else
                        {
                            address_select.addressInfo = res.data.addressInfo;
                            //if the user is already login and not passed from login, set in session
                            HttpContext.Session.SetObjectAsJson("PersonalDetails", res, authTime);

                        }
                    } else
                    {
                        address_select.addressInfo = res.data.addressInfo;
                    }

                }
            }
            return Page();
        }
        public IActionResult OnPost(bool review)
        {

            //Set class Model before validation
            if (crbAddress == "1")
            {
                address_select.use_from_civil = true;
                address_select.use_other = false;
            }
            else if (crbAddress == "2")
            {
                address_select.use_from_civil = false;
                address_select.use_other = true;
            }
            else
            {
                address_select.use_from_civil = false;
                address_select.use_other = false;
            }

            //Validate Model
            FluentValidation.Results.ValidationResult result = _validator.Validate(address_select);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                return RedirectToPage("Address", null, new { fromPost = true }, "mainContainer");
            }
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            //Model is valid so strore 
            HttpContext.Session.SetObjectAsJson("AddressSelect", address_select, authTime);
            //Remove Error Session 
            HttpContext.Session.Remove("valresult");
            //Finally redirect
            //Set the Back and Next Link
            if (address_select.use_other)
            {
                NextLink = _nav.SetLinks("address-selection", "Address", review, "No");
            }
            else
            {
                NextLink = _nav.SetLinks("address-selection", "Address", review, "Yes");
            }
            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review });
            }
            else
            {
                return RedirectToPage(NextLink);
            }
        }
    }

}
