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

namespace dsf_service_template_net6.Pages
{
    public class AddressEditModel : PageModel
    {
        #region "Variables"
        //Dependancy injection Variables
        private readonly IStringLocalizer _localizer;
        private readonly IValidator<AddressEditViewModel> _validator;
        public readonly IMyHttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AddressEditModel> _logger;
        //Form Controls
        [BindProperty]
        public string PostalCodeErrorClass { get; set; } = "";
        #endregion
        #region "Custom Methods"
        private List<Addressinfo> AddressesForPostalCode
        {
            get
            {
                return GetAddressesForPostalCode();
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

                //Update Error messages on View
                ClearErrors();
                res.AddToModelState(this.ModelState, "AddressEditViewModel");
                CreateErrorSummary(res);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void SetViewModelAddresses()
        {
            ViewModel.Addresses = GetAddressesForPostalCode()
                .Where(i => i != null)
                .Select(i =>
                {
                    string addressText = i.addressText + ", " + i.parish.name + ", " + i.district.name;
                    return new SelectListItem(addressText, i.item.code.ToString());
                }).ToList();

            ViewModel.Addresses.Insert(0, new SelectListItem(_localizer.GetString("PleaseSelectAddress"), ""));
        }
        public List<Addressinfo> GetAddressesForPostalCode()
        {
            List<Addressinfo> addressesForPostalCode = null;
            if (ViewModel.postalCode.Length > 0)
            {
                addressesForPostalCode = HttpContext.Session.GetObjectFromJson<List<Addressinfo>>("AddressesForPostalCode");
                //we cache the date in session for this mock
                if (addressesForPostalCode == null)
                {
                    string response;
                    string lang = GetLanguage();
                    string apiUrl = "api/v1/MoiCrmd/address-mock/" + ViewModel.postalCode + lang;
                    try
                    {
                        // populate dropdown
                        response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiUrl, "");
                        AddressEdit apiResponse = JsonConvert.DeserializeObject<AddressEdit>(response) ?? new AddressEdit();
                        if (apiResponse.succeeded & apiResponse.data != null)
                        {
                            addressesForPostalCode = apiResponse.data?.items.Select(i =>
                            {
                                return new Addressinfo()
                                {
                                    postalCode = apiResponse.data?.postalCode ?? int.Parse(ViewModel.postalCode),
                                    addressText = i.name,
                                    language = lang,
                                    addressVerified = true, //true after OTP verification
                                    country = apiResponse.data?.country,
                                    district = apiResponse.data?.district,
                                    parish = apiResponse.data?.parish,
                                    town = apiResponse.data?.town,
                                    type = "mail_address",
                                    item = new() { code = i.code, name = i.name },
                                };
                            }).ToList();
                            HttpContext.Session.SetObjectAsJson("AddressesForPostalCode", addressesForPostalCode); ;
                        }
                    }
                    catch
                    {
                        _logger.LogError("Could not get valid response from " + apiUrl);
                    }
                }
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
            //if (HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime) == null)
            //{
            //    ret = false;
            //}
            //if (HttpContext.Session.GetObjectFromJson<AddressSelect>("AddressSelect", authTime) == null)
            //{
            //    ret = false;
            //}
            //if ((HttpContext.Session.GetObjectFromJson<MobileSelect>("MobileSelect", authTime) == null) && (HttpContext.Session.GetObjectFromJson<MobileEdit>("MobEdit", authTime) == null))
            //{
            //    ret = false;
            //}
            return ret;
        }

        private void CreateSubmitData()
        {
            //we should also filter using Parish
            Addressinfo addressEdit = AddressesForPostalCode
                .First(i => i.item.code == int.Parse(ViewModel.SelectedAddress));

            addressEdit.item.street = new()
            {
                streetNumber = ViewModel.StreetNo,
                apartmentNumber = ViewModel.FlatNo
            };

            addressEdit.addressText = BuildAddressText(addressEdit);

            //Nr we use authenticate time for encrypting and decrypting the data
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;

            HttpContext.Session.SetObjectAsJson("AddressEdit", addressEdit, authTime);
        }

        private string BuildAddressText(Addressinfo addressEdit)
        {
            return addressEdit.addressText = addressEdit.item.name + " " + addressEdit.item.street.streetNumber + " " +
                (!string.IsNullOrEmpty(addressEdit.item.street.apartmentNumber) && GetLanguage() == "/el"
                    ? "ΔΙΑΜ. " + addressEdit.item.street.apartmentNumber
                    : "APARTEMENT NO. " + addressEdit.item.street.apartmentNumber) + "\n"
                + addressEdit.town.name + " " + addressEdit.district.name + "\n" + addressEdit.country.name;
        }

        private void ClearErrors()
        {
            ViewModel.PostalCodeFormClass = ViewModel.FormClassNoError;
            ViewModel.PostalCodeTextboxCSS = ViewModel.PostalCodeTextboxCSSNoError;
            ViewModel.FlatNoTextboxCSS = ViewModel.FlatNoTextboxCSSNoError;
        }

        private void CreateErrorSummary(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                ViewModel.ErrorDesc += "<a href='#ViewModel_" + error.PropertyName + "'>" + error.ErrorMessage + "</a>";
            }
            ViewModel.ShowErrorSummary = true;
        }
        #endregion
        public AddressEditModel(IStringLocalizer<AddressEdit> localizer, IValidator<AddressEditViewModel> validator, IMyHttpClient client, IConfiguration config, ILogger<AddressEditModel> logger)
        {
            _localizer = localizer;
            _validator = validator;
            _client = client;
            _configuration = config;
            _logger = logger;
        }

        [BindProperty]
        public AddressEditViewModel ViewModel { get; set; } = new();

        /// <summary>
        /// Store 
        /// </summary>

        public IActionResult OnGet()
        {
            //Check if user has sequentialy load the page
            bool allow = AllowToProceed();
            if (!allow)
            {
                return RedirectToAction("LogOut", "Account");
            }

            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            Addressinfo addressFromSession = null;
            if (ShowErrors())
            {
                ViewModel.HasUserEnteredPostalCode = HttpContext.Session.GetObjectFromJson<bool?>("HasUserEnteredPostalCode") ?? false;
                ViewModel.HasUserSelectedAddress = HttpContext.Session.GetObjectFromJson<bool?>("HasUserSelectedAddress") ?? false;
                ViewModel.SelectedAddress = HttpContext.Session.GetObjectFromJson<string?>("SelectedAddress") ?? "";
                ViewModel.postalCode = HttpContext.Session.GetObjectFromJson<string?>("SelectedPostalCode") ?? "";

                addressFromSession = AddressesForPostalCode.First(a => a.item.code == int.Parse(ViewModel.SelectedAddress));

            }
            else
            {
                addressFromSession = HttpContext.Session.GetObjectFromJson<Addressinfo>("AddressEdit", authTime);
            }
            if (addressFromSession != null)
            {
                ViewModel.postalCode = addressFromSession.postalCode.ToString();
                ViewModel.StreetNo = addressFromSession.item.street?.streetNumber ?? "";
                ViewModel.FlatNo = addressFromSession.item.street?.apartmentNumber ?? "";
                SetViewModelAddresses();//ViewModel.Addresses
                ViewModel.SelectedAddressName = addressFromSession.addressText;
                ViewModel.City = addressFromSession.district.name;
                ViewModel.Parish = addressFromSession.parish.name;
                ViewModel.SelectedAddress = addressFromSession.item.code.ToString();
                ViewModel.HasUserEnteredPostalCode = true;
                ViewModel.HasUserSelectedAddress = true;
            }
            else if (HttpContext.Session.GetObjectFromJson<string>("SelectedPostalCode") != null)
            {
                ViewModel.postalCode = HttpContext.Session.GetObjectFromJson<string?>("SelectedPostalCode") ?? "";
                ViewModel.HasUserEnteredPostalCode = true;
                //try to bind the list if it has Data

                if (HttpContext.Session.GetObjectFromJson<List<SelectListItem>>("ResultList") != null)
                {
                    ViewModel.Addresses = HttpContext.Session.GetObjectFromJson<List<SelectListItem>>("ResultList");
                }
            }
            return Page();
        }
        public IActionResult OnPostPostalCode()
        {
            ViewModel.HasUserEnteredPostalCode = true;
            HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);
            ValidationResult result = _validator.Validate(ViewModel);
            if (result.IsValid)
            {
                ViewModel.PostalCodeFormClass = ViewModel.FormClassNoError;
                ViewModel.PostalCodeTextboxCSS = ViewModel.PostalCodeTextboxCSSNoError;
                ViewModel.FlatNoTextboxCSS = ViewModel.FlatNoTextboxCSSNoError;
                ViewModel.ShowErrorSummary = false;
                SetViewModelAddresses();
                HttpContext.Session.SetObjectAsJson("ResultList", ViewModel.Addresses);
                ClearErrors();
            }
            else
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("HasUserEnteredPostalCode", ViewModel.HasUserEnteredPostalCode);
                return RedirectToPage("AddressEdit");
            }
           return RedirectToPage("AddressEdit");
        }
        public void OnPostSelectAddressFromDropDown()
        {
            ViewModel.HasUserEnteredPostalCode = true;
            ViewModel.HasUserSelectedAddress = true;

            ClearErrors();
            SetViewModelAddresses();
            Addressinfo selectedAddressInfo = AddressesForPostalCode.First(a => a.item.code == int.Parse(ViewModel.SelectedAddress));
            ViewModel.SelectedAddressName = selectedAddressInfo.addressText;
            ViewModel.City = selectedAddressInfo.town.name ?? "";
            ViewModel.Parish = selectedAddressInfo.parish.name ?? "";
        }
        public IActionResult OnPostVerifyAddress(bool review)
        {
            ViewModel.HasUserEnteredPostalCode = true;
            ViewModel.HasUserSelectedAddress = true;
            ValidationResult result = _validator.Validate(ViewModel);
            if (!result.IsValid)
            {
                HttpContext.Session.SetObjectAsJson("valresult", result);
                HttpContext.Session.SetObjectAsJson("HasUserEnteredPostalCode", ViewModel.HasUserEnteredPostalCode);
                HttpContext.Session.SetObjectAsJson("HasUserSelectedAddress", ViewModel.HasUserSelectedAddress);
                HttpContext.Session.SetObjectAsJson("SelectedAddress", ViewModel.SelectedAddress);
                HttpContext.Session.SetObjectAsJson("SelectedPostalCode", ViewModel.postalCode);

                return RedirectToPage("AddressEdit");
            }

            CreateSubmitData();

            //clear temporary session for this page
            HttpContext.Session.Remove("valresult");
            HttpContext.Session.Remove("AddressesForPostalCode");
            HttpContext.Session.Remove("HasUserEnteredPostalCode");
            HttpContext.Session.Remove("HasUserSelectedAddress");
            HttpContext.Session.Remove("SelectedAddress");
            HttpContext.Session.Remove("SelectedPostalCode");

            //Finall redirect NR code addition
            //Re-assign defult adressInfo
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            var citizenData = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
            if (review)
            {
                return RedirectToPage("/ReviewPage", null, "mainContainer");
            }
            else if (string.IsNullOrEmpty(citizenData.data.mobile))
            {
                return RedirectToPage("/MobileEdit", null, "mainContainer");
            }
            else
            {
                return RedirectToPage("/Mobile", null, "mainContainer");
            }
        }
       
    }
}
