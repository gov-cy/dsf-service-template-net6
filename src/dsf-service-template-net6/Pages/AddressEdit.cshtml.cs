
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

       
        [BindProperty]
        [Required(ErrorMessage ="Τ.Κ Υποχεωτικό")]
        [MinLength(4, ErrorMessage ="Το Τ.Κ παίρνει πάντα 4 χαρακτήρες")]
        [MaxLength(4, ErrorMessage = "Το Τ.Κ παίρνει πάντα 4 χαρακτήρες")]
        public string PostalCode { get; set; }

        [BindProperty]
        [Required(ErrorMessage ="Αριθμός Οδού Υποχρεωτικός")]        
        public string StreetNo { get; set; }

        [BindProperty]
        public string FlatNo { get; set; }


        Item AddressData = new();

        public int AddressSelected { get; set; }
        public string FormClassNoError { get; set; } = "govcy-form-control";
        public string FormClassWithError { get; set; } = "govcy-form-control govcy-form-control-error";        
        public string PostalCodeFormClass { get; set; } = "govcy-form-control";

        public string VerifyAddressFormClass { get; set; } = "govcy-form-control";


        public bool HasUserEnteredPostalCcode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;
        public AddressSummary AddressSummary { get; set;}      
        public AddressesMain? Addressinfo { get; set; }

        public void OnGet()
        {
            PostalCodeFormClass = FormClassNoError;
        }

        public void OnPost()
        {
            
        }
       
        public IActionResult OnPostView()
        {
            var val = ModelState.FirstOrDefault(o => o.Key == "PostalCode").Value.ValidationState;// = ModelValidationState.Valid;
            if (val == ModelValidationState.Valid) // if (!string.IsNullOrEmpty(PostalCode) && PostalCode.Length==4)
            {
                HttpContext.Session.SetString("PostalCode", PostalCode);
                GetAddressesForPostalCode();
                PostalCodeFormClass = FormClassNoError;              
            }
            else
            {
              //  ModelState.AddModelError("PostalCode", "PostalCode  is a required field.");                
                PostalCodeFormClass = FormClassWithError;                            
            }
            return Page();
        }

        public void OnPostSelectAddressFromDropDown(int addressCode)
        {
            ModelState.ClearValidationState("PostalCode");
            ModelState.ClearValidationState("StreetNo");
            AddressSelected = addressCode;
            HttpContext.Session.SetInt32("AddressSelected", AddressSelected);

            if (HttpContext.Session.GetString("PostalCode") != null)
            { 
                PostalCode = HttpContext.Session.GetString("PostalCode");
                HasUserSelectedAddress = true;
                GetAddressesForPostalCode();
                FillCitizenAddressTable(addressCode);
            }            
        }

        public IActionResult OnPostVerifyAddress()
        {

            var val = ModelState.FirstOrDefault(o => o.Key == "StreetNo").Value.ValidationState;// = ModelValidationState.Valid;
            ModelState.ClearValidationState("PostalCode");
            AddressSelected = (int)HttpContext.Session.GetInt32("AddressSelected"); ;
            PostalCode = HttpContext.Session.GetString("PostalCode");
            HasUserSelectedAddress = true;
            GetAddressesForPostalCode();
            FillCitizenAddressTable(AddressSelected);
            if (val == ModelValidationState.Valid)
            {
                VerifyAddressFormClass = FormClassNoError;
                CreateSubmitData();
                return RedirectToPage("/Mobile");
            }
            else
            {
                VerifyAddressFormClass = FormClassWithError;
                return Page();
            }
           
        }
        public void GetAddressesForPostalCode()
        {
            if (PostalCode.Length>0)
            {
                string apiURL = "api/v1/MoiCrmd/address-mock/" + PostalCode + "/el";
                var response = _client.MyHttpClientGetRequest(_configuration["ApiUrl"], apiURL, "");
              
                try
                {
                    // populate dropdown                   
                    Addressinfo = JsonConvert.DeserializeObject<AddressesMain>(response);                  
                    HasUserEnteredPostalCcode = true;                  
                }
                catch
                {

                }
            }
        }

        public void FillCitizenAddressTable(int adddressCode)
        {
            AddressData = new Item();
            AddressData =  Addressinfo.data.items.ToList().First(address => address.code == adddressCode);

            AddressSummary = new AddressSummary();
            AddressSummary.Street = AddressData.name;
            AddressSummary.Parish = Addressinfo.data.parish.name;
            AddressSummary.City = Addressinfo.data.district.name;          
        }  
        
        private void CreateSubmitData()
        {
            Addressinfo addressFinal = new Addressinfo();
            addressFinal.postalCode = Convert.ToInt32( PostalCode);
            addressFinal.language = "el";
            addressFinal.item = AddressData;
            addressFinal.town = Addressinfo.data.town;
            addressFinal.parish = Addressinfo.data.parish;
            addressFinal.district = Addressinfo.data.district;
            addressFinal.country = Addressinfo.data.country;

            HttpContext.Session.SetObjectAsJson("AddressEdit", addressFinal);

           
        }
    }
}
