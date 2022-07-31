
using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using dsf_service_template_net6.Extensions;

namespace dsf_service_template_net6.Pages
{

    public class AddressEditModel : PageModel
    {
        private IMyHttpClient _client;
        private IConfiguration _configuration;

        public AddressEditModel(IConfiguration configuration, IMyHttpClient client)
        {
            _client = client;
            _configuration = configuration;
        }

        public bool ShowErrorSummary { get; set; } = false;

        [BindProperty]
        [Required(ErrorMessage = "Τ.Κ Υποχεωτικό")]
        [MinLength(4, ErrorMessage = "Το Τ.Κ παίρνει πάντα 4 χαρακτήρες")]
        [MaxLength(4, ErrorMessage = "Το Τ.Κ παίρνει πάντα 4 χαρακτήρες")]
        public string PostalCode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Αριθμός Οδού Υποχρεωτικός")]
        public string StreetNo { get; set; }

        [BindProperty]
        public string FlatNo { get; set; }


        Item AddressData = new();

        #region vars
        public string ErrorDesc = "";
        public int AddressSelected { get; set; }
        public string FormClassNoError { get; set; } = "govcy-form-control";
        public string FormClassWithError { get; set; } = "govcy-form-control govcy-form-control-error";
        public string PostalCodeFormClass { get; set; } = "govcy-form-control";
        public string VerifyAddressFormClass { get; set; } = "govcy-form-control";
        public string PostalCodeTextboxCSSNoError { get; set; } = "govcy-text-input govcy-text-input-char_4";
        public string PostalCodeTextboxCSSWithError { get; set; } = "govcy-text-input govcy-text-input-char_4 govcy-text-input-error";        
        public string PostalCodeTextboxCSS { get; set; }
        public string FlatNoTextboxCSS { get; set; }
        public string FlatNoTextboxCSSNoError { get; set; } = "govcy-text-input govcy-text-input-char_5";
        public string FlatNoTextboxCSSWithError { get; set; } = "govcy-text-input govcy-text-input-char_5 govcy-text-input-error";       
        public bool HasUserEnteredPostalCcode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;

        public AddressSummary AddressSummary { get; set; }
        public AddressesMain? Addressinfo { get; set; }

        #endregion
        public void OnGet()
        {
            // 1. check if comong from other pages
           
            GetDataFromSession("AddressEdit");

            // 2. Setup CSS for errors
            ClearErrors();

        }

        public void OnPost()
        {

        }

        public IActionResult OnPostView()
        {
            var val = ModelState.FirstOrDefault(o => o.Key == "PostalCode").Value;// = ModelValidationState.Valid;
            if (val.ValidationState == ModelValidationState.Valid) // if (!string.IsNullOrEmpty(PostalCode) && PostalCode.Length==4)
            {
                ClearErrors();
                if (GetAddressesForPostalCode())
                {
                    HttpContext.Session.SetString("PostalCode", PostalCode);

                    PostalCodeFormClass = FormClassNoError;
                    PostalCodeTextboxCSS = PostalCodeTextboxCSSNoError;
                    FlatNoTextboxCSS = FlatNoTextboxCSSNoError;
                    ShowErrorSummary = false;
                }
            }
            else
            {              
                PostalCodeFormClass = FormClassWithError;
                PostalCodeTextboxCSS = PostalCodeTextboxCSSWithError;
                FlatNoTextboxCSS = FlatNoTextboxCSSWithError;

                CreateErrorSummary(val);                                   
                           
            }
            return Page();
        }

        public void OnPostSelectAddressFromDropDown(int addressCode)
        {
            ModelState.ClearValidationState("PostalCode");
            ModelState.ClearValidationState("StreetNo");
            AddressSelected = addressCode;
            HttpContext.Session.SetInt32("AddressSelected", AddressSelected);
            PostalCodeTextboxCSS = PostalCodeTextboxCSSNoError;
            FlatNoTextboxCSS = FlatNoTextboxCSSNoError;


            if (HttpContext.Session.GetString("PostalCode") != null)
            {
                PostalCode = HttpContext.Session.GetString("PostalCode");
                HasUserSelectedAddress = true;
                GetAddressesForPostalCode();
                FillCitizenAddressTable(addressCode);
            }
        }

        public IActionResult OnPostVerifyAddress(bool review)
        {

            var val = ModelState.FirstOrDefault(o => o.Key == "StreetNo").Value;// = ModelValidationState.Valid;
            ModelState.ClearValidationState("PostalCode");
            AddressSelected = (int)HttpContext.Session.GetInt32("AddressSelected"); ;
            PostalCode = HttpContext.Session.GetString("PostalCode");
            HasUserSelectedAddress = true;
            GetAddressesForPostalCode();
            FillCitizenAddressTable(AddressSelected);
            PostalCodeTextboxCSS = PostalCodeTextboxCSSNoError;
            if (val.ValidationState == ModelValidationState.Valid)
            {
                VerifyAddressFormClass = FormClassNoError;                
                FlatNoTextboxCSS = FlatNoTextboxCSSNoError;
                ShowErrorSummary = false;
                CreateSubmitData();
                //Finall redirect NR code addition
                //Re-assign defult adressInfo
                var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
                var citizen_data = HttpContext.Session.GetObjectFromJson<CitizenDataResponse>("PersonalDetails", authTime);
                if (review)
                {
                    return RedirectToPage("/ReviewPage", null, "mainContainer");
                } 
                else if (string.IsNullOrEmpty(citizen_data.data.mobile))
                {
                        return RedirectToPage("/MobileEdit", null, "mainContainer");
                }
                else
                {
                        return RedirectToPage("/Mobile", null, "mainContainer");
                }
                  
            }
            else
            {
                
                VerifyAddressFormClass = FormClassWithError;
                FlatNoTextboxCSS = FlatNoTextboxCSSWithError;                
                CreateErrorSummary(val);
                
                return Page();
            }

        }
        public bool GetAddressesForPostalCode()
        {
         //   return false;
            bool dataFound = false;
            if (PostalCode.Length > 0)
            {
                string apiURL = "api/v1/MoiCrmd/address-mock/" + PostalCode + "/el";
                var response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiURL, "");
                if (response != null)
                {
                    try
                    {
                        // populate dropdown                   
                        Addressinfo = JsonConvert.DeserializeObject<AddressesMain>(response);
                        HasUserEnteredPostalCcode = true;
                        dataFound = true;
                    }
                    catch
                    {

                    }
                }
                else
                {


                }


            }
            return dataFound;
        }


        public void FillCitizenAddressTable(int adddressCode)
        {
            AddressData = new Item();
            AddressData = Addressinfo.data.items.ToList().First(address => address.code == adddressCode);

            AddressSummary = new AddressSummary();
            AddressSummary.Street = AddressData.name;
            AddressSummary.Parish = Addressinfo.data.parish.name;
            AddressSummary.City = Addressinfo.data.district.name;
            AddressSummary.StreetNumber = StreetNo;
        }

        private void CreateSubmitData()
        {
            Addressinfo addressFinal = new Addressinfo();
            addressFinal.postalCode = Convert.ToInt32(PostalCode);
            addressFinal.language = "el";
            Street street = new()
            {
                streetNumber = StreetNo,
                apartmentNumber = FlatNo
            };

            addressFinal.item = new();
            addressFinal.item = AddressData;
            addressFinal.item.street = new(); //.streetNumber = street.streetNumber;
            addressFinal.item.street = street;
           
            addressFinal.town = Addressinfo.data.town;
            addressFinal.parish = Addressinfo.data.parish;
            addressFinal.district = Addressinfo.data.district;
            addressFinal.country = Addressinfo.data.country;
           //Nr we use authenticate time for encrypting and decrypting the data
            var authTime = User.Claims.First(c => c.Type == "auth_time").Value;
            HttpContext.Session.SetObjectAsJson("AddressEdit", addressFinal,authTime);
        }

        private void GetDataFromSession(string key)
        {
            if (HttpContext.Session.GetObjectFromJson<Addressinfo>(key)!=null)
            {
                Addressinfo addressFromSession = new();
                addressFromSession = (Addressinfo)HttpContext.Session.GetObjectFromJson<Addressinfo>(key);
                PostalCode =  addressFromSession.postalCode.ToString();
                StreetNo = addressFromSession.item.street.streetNumber;
                FlatNo = addressFromSession.item.street.apartmentNumber;
                HasUserEnteredPostalCcode = true;
                HasUserSelectedAddress = true;
                AddressSelected = (int)HttpContext.Session.GetInt32("AddressSelected"); ;
                GetAddressesForPostalCode();
                FillCitizenAddressTable(AddressSelected);
            }
        }

        private void ClearErrors()
        {
            PostalCodeFormClass = FormClassNoError;
            PostalCodeTextboxCSS = PostalCodeTextboxCSSNoError;
            FlatNoTextboxCSS = FlatNoTextboxCSSNoError;
        }

        private void CreateErrorSummary(ModelStateEntry val)
        {
            var _errorList = ModelState.Values;

            foreach (var error in val.Errors)
            {
                ErrorDesc += error.ErrorMessage;
            }
            ShowErrorSummary = true;
        }
    }
}
