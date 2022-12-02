using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
//using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.Extensions.Localization;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Extensions;
using System.Collections.Generic;
using dsf_service_template_net6.Pages.Shared;

namespace dsf_service_template_net6.Pages
{
    public class AddressEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly INavigation _nav;
        private readonly IMoiCrmd _service;
        private readonly IValidator<AddressEditViewModel> _validator;

        //Form Controls
        [BindProperty]
        public string PostalCodeErrorClass { get; set; } = "";
        [BindProperty]
        public string StreetErrorClass { get; set; } = "";
        [BindProperty]
        public string FlatErrorClass { get; set; } = "";
        [BindProperty]
        public string BackLink { get; set; } = "";
       
        [BindProperty]
        public string NextLink { get; set; } = "";
        [BindProperty]
        public bool DisplayNonFoundInstructions { get; set; } = false;
        #endregion
        public AddressEditModel(IValidator<AddressEditViewModel> validator, IMoiCrmd service, INavigation nav)
        {
            _validator = validator;
            _service = service;
            _nav = nav;
        }

        #region "Custom Methods"


        private List<Addressinfo> AddressesForPostalCode
        {
            get
            {
                return GetAddressesForPostalCode();
            }
        }

        bool ShowErrors(bool fromPost)
        {
            
            if (fromPost)
            {
                
                var res = HttpContext.Session.GetObjectFromJson<ValidationResult>("valresult");
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                res.AddToModelState(ModelState, "ViewModel");
                ClearErrors();
                //Update Error messages on View
                CreateErrorSummary(res);
                return true;
            }
            return false;
        }

        private List<SelectListItem> GetViewModelAddresses()
        {
            List<Addressinfo> addressList = GetAddressesForPostalCode();

            List<SelectListItem> addressDropDown = addressList.Where(i => i != null)
                .Select(i =>
                {
                    string addressText = i.addressText + (i.parish.name == "." ? "" : ", " + i.parish.name) + ", " + i.town.name; ;
                    return new SelectListItem(addressText, i.item.code.ToString());
                }).ToList();
            return addressDropDown;
        }
        public List<Addressinfo> GetAddressesForPostalCode()
        {
            List<Addressinfo> addressesForPostalCode = null;
            if (ViewModel?.postalCode?.Length > 0)
            {
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                string lang = GetLanguage();
                addressesForPostalCode = _service.GetAdressesForPostalCode(lang, ViewModel.postalCode);
            }
            return addressesForPostalCode;
        }

        private static string GetLanguage()
        {
            return Thread.CurrentThread.CurrentUICulture.Name == "el-GR" ? "/el" : "/en";
        }

        private bool AllowToProceed()
        {
            bool ret = true;
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            {
                ret = false;
            }
          
            return ret;
        }

        private void CreateSubmitData()
        {
            //we should also filter using Parish
            Addressinfo addressEdit = AddressesForPostalCode.First(i => i.item.code == ViewModel.SelectedAddress);

            addressEdit.item.street = new()
            {
                streetNumber = ViewModel.StreetNo,
                apartmentNumber = ViewModel.FlatNo
            };
            addressEdit.addressText = BuildAddressText(addressEdit);

            //Nr we use authenticate time for encrypting and decrypting the data
            HttpContext.Session.SetObjectAsJson("AddressEdit", addressEdit, User.Claims.First(c => c.Type == "auth_time").Value);
        }

        private static string BuildAddressText(Addressinfo addressEdit)
        {
            string appart = "";
            if (!string.IsNullOrEmpty(addressEdit.item.street.apartmentNumber))
            {
                if (GetLanguage() == "/el")
                {
                    appart = "ΔΙΑΜ. ";
                }
                else
                {
                    appart = "APARTEMENT NO. ";
                }

            }
            return addressEdit.addressText = addressEdit.item.name + " " + addressEdit.item.street.streetNumber + " " +
               (!string.IsNullOrEmpty(addressEdit.item.street.apartmentNumber)
                   ? appart + addressEdit.item.street.apartmentNumber
                   : ""
               ) + "\n" + addressEdit.postalCode + " " + addressEdit.town.name + "\n" + addressEdit.district.name + "\n" + addressEdit.country.name;
        }

        private void ClearErrors()
        {
            //Clear form errors
            foreach (var modelValue in ModelState.Values)
            {
                modelValue.Errors.Clear();
            }
            PostalCodeErrorClass = "";
            StreetErrorClass = "";
            FlatErrorClass = "";
            HttpContext.Session.Remove("valresult");
            ViewModel.ShowErrorSummary = false;
            ViewModel.NoResultsFound = false;
        }

        private void CreateErrorSummary(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.PropertyName == "postalCode")
                {
                    ViewModel.ErrorDesc += "<a href='#PostalCode'>" + error.ErrorMessage + "</a>";
                    PostalCodeErrorClass = error.ErrorMessage;
                    ViewModel.ShowErrorSummary = true;
                }
                if (error.PropertyName == "StreetNo")
                {
                    ViewModel.ErrorDesc += "<a href='#StreetNo'>" + error.ErrorMessage + "</a>";
                    StreetErrorClass = error.ErrorMessage;
                    ViewModel.ShowErrorSummary = true;
                }
                if (error.PropertyName == "FlatNo")
                {
                    ViewModel.ErrorDesc += "<a href='#FlatNo'>" + error.ErrorMessage + "</a>";
                    FlatErrorClass = error.ErrorMessage;
                    ViewModel.ShowErrorSummary = true;
                }
                if (error.PropertyName == "Addresses")
                {
                    ViewModel.NoResultsFound = true;
                }
            }
           
        }
        #endregion

        [BindProperty]
        public AddressEditViewModel ViewModel { get; set; } = new();

        /// <summary>
        /// Get Method use to load the content for each post
        /// </summary>
        public IActionResult OnGet(bool review, bool fromPost)
        {              
            //Check if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }
            //Set Back Link
            BackLink = _nav.GetBackLink("/set-address", review);
            //Get Data from session
            Addressinfo addressFromSession = HttpContext.Session.GetObjectFromJson<Addressinfo>("AddressEdit", User.Claims.First(c => c.Type == "auth_time").Value);
            //Store object in temp session ,
            //so that any changes made without save will not taken into account if press back
            if (addressFromSession != null)
            {
                HttpContext.Session.SetObjectAsJson("AddressEditTemp", addressFromSession, User.Claims.First(c => c.Type == "auth_time").Value);
            }
            //keep incorect typed values
            if (fromPost)
            {
                ViewModel.postalCode = HttpContext.Session.GetObjectFromJson<string?>("SelectedPostalCode") ?? "";
                ViewModel.SelectedAddress = HttpContext.Session.GetObjectFromJson<string?>("SelectedAddress") ?? "";
                ViewModel.StreetNo = HttpContext.Session.GetObjectFromJson<string?>("TypeStreetNo") ?? "";
                ViewModel.FlatNo = HttpContext.Session.GetObjectFromJson<string?>("TypeFlatNo")?.ToString() ?? "";
            }
            //Show Revisited values
            else
            {
                ViewModel.postalCode = addressFromSession?.postalCode.ToString() ?? "";
                ViewModel.SelectedAddress = addressFromSession?.item.code.ToString() ?? "";
                ViewModel.StreetNo = addressFromSession?.item.street?.streetNumber ?? "";
                ViewModel.FlatNo = addressFromSession?.item.street?.apartmentNumber ?? "";
            }
            ViewModel.HasUserEnteredPostalCode = !string.IsNullOrEmpty(ViewModel.postalCode);
            ViewModel.HasUserSelectedAddress = !string.IsNullOrEmpty(ViewModel.SelectedAddress);
             
         
            //if user has selected adress
             if (ViewModel.HasUserSelectedAddress)
            {
                if (addressFromSession == null)
                    addressFromSession = AddressesForPostalCode.First(a => a.item.code == ViewModel.SelectedAddress);
                ViewModel.SelectedAddressName = addressFromSession.item.name;
                ViewModel.City = addressFromSession.district.name;
                ViewModel.Town = addressFromSession.town.name;
                ViewModel.Parish = addressFromSession.parish.name;
            }
            //if only search from postal code make check
            if (ViewModel.HasUserEnteredPostalCode)
            {
                //Call the Api
                ViewModel.Addresses = GetViewModelAddresses() ;
                if (!ViewModel.HasUserSelectedAddress)
                {
                    DisplayNonFoundInstructions = true;
                }
            }
            var FormHasErrors = false;
            if (fromPost)
            {
                FormHasErrors = ShowErrors(fromPost);
            }
            if (FormHasErrors && !ViewModel.HasUserSelectedAddress)
            {
                //Remove list
                ViewModel.Addresses?.Clear();

            }
            if (ViewModel.postalCode == "")
            {
                ViewModel.postalCode = HttpContext.Session.GetObjectFromJson<string?>("SelectedPostalCodeOnly") ?? "";
            }
            return Page();
        }

        /// <summary>
        /// Called when use enters postal code
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostPostalCode()
        {
            //1-Initialize the user selection first
            HttpContext.Session.Remove("SelectedAddress");
            HttpContext.Session.Remove("SelectedPostalCodeOnly");
            HttpContext.Session.Remove("SelectedPostalCode");
            //Validate only postal code
            ValidationResult result_for_postal = _validator.Validate(ViewModel);
            if (!result_for_postal.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result_for_postal);
                HttpContext.Session.SetObjectAsJson("SelectedPostalCodeOnly", ViewModel.postalCode);
                HttpContext.Session.Remove("AddressEditTemp");
                return RedirectToPage("AddressEdit", null, new { fromPost = true }, "mainContainer");
            }
            //Get The drop down list before validation
            if (!string.IsNullOrEmpty(ViewModel.postalCode))
            {
                ViewModel.Addresses= GetViewModelAddresses();
            }
            ViewModel.HasUserEnteredPostalCode = true;
            ViewModel.HasUserSelectedAddress = false;
            //2-Validate
            ValidationResult result = _validator.Validate(ViewModel);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                //Remove Previous List if Exists
                HttpContext.Session.Remove("AddressesForPostalCode");
                HttpContext.Session.Remove("AddressEditTemp");
                //Set the invalid postal code
                HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);
                return RedirectToPage("AddressEdit", null, new { fromPost = true }, "mainContainer");
            }
            else
            {
                //Clear previous errors
                ClearErrors();

                //Remove previous session storage on change address
                HttpContext.Session.Remove("AddressEditTemp");
                //Set the entered postal code
                HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);
            }
           
            //Generate a Get Method
            return RedirectToPage("AddressEdit", new { fromPost = true });
        }

        public IActionResult OnPostSelectAddressFromDropDown()
        {
            //remove first
            HttpContext.Session.Remove("AddressEditTemp");
            HttpContext.Session.SetObjectAsJson("TypeStreetNo", "");
            HttpContext.Session.SetObjectAsJson("TypeFlatNo", "");
            //Need to store the selected address code
            HttpContext.Session.SetObjectAsJson("SelectedAddress", ViewModel.SelectedAddress);
            HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);

            return RedirectToPage("AddressEdit", new { fromPost = true });
        }

        /// <summary>
        /// if Valid move to next page , after session save
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        public IActionResult OnPostVerifyAddress(bool review)
        {
            ViewModel.HasUserEnteredPostalCode = true;
            ViewModel.HasUserSelectedAddress = true;
           
            ValidationResult result = _validator.Validate(ViewModel);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("SelectedAddress", ViewModel.SelectedAddress);
                HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);
                HttpContext.Session.SetObjectAsJson("TypeStreetNo", ViewModel.StreetNo);
                HttpContext.Session.SetObjectAsJson("TypeFlatNo", ViewModel.FlatNo);
                return RedirectToPage("AddressEdit", null, new { fromPost = true }, "mainContainer");
            }

            CreateSubmitData();

            //clear temporary session for this page
            HttpContext.Session.Remove("AddressEditTemp");
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("AddressesForPostalCode");
            HttpContext.Session.Remove("SelectedAddress");
            HttpContext.Session.Remove("SelectedPostalCode");
            HttpContext.Session.Remove("TypeStreetNo");
            HttpContext.Session.Remove("TypeFlatNo");
          
            //Set back and Next Link
            NextLink = _nav.SetLinks("set-address", "Address", review, "NoSelection");
            if (review)
            {
                return RedirectToPage(NextLink, null, new { review = review }, "mainContainer");
            }else
            {
                return RedirectToPage(NextLink);
            }
           
            
        }
    }
}
