
using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;

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

        [BindProperty, Required(ErrorMessage = "Postal Code required")]
                
        public string PostalCode { get; set; }


        [BindProperty]
        public int AddressSelected { get; set; }


        public string FormClassNoError { get; set; } = "govcy-form-control";
        public string FormClassWithError { get; set; } = "govcy-form-control govcy-form-control-error";        
        public string FormClass { get; set; } = "govcy-form-control";


        public bool HasUserEnteredPostalCcode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;
        public AddressSummary addressSummary { get; set;}      
        public AddressesMain? addressinfo { get; set; }

        public void OnGet()
        {
            FormClass = FormClassNoError;
        }

        public void OnPost()
        {
           
        }
       
        public IActionResult OnPostView()
        {

            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("PostalCode", PostalCode);
                GetAddressesForPostalCode();
                FormClass = FormClassNoError;
                return Page();
            }
            else
            {
                FormClass = FormClassWithError;
                return Page();
              
            }
        }

        public void OnPostSelectAddressFromDropDown(int addressCode)
        {
            AddressSelected = addressCode;
            if (HttpContext.Session.GetString("PostalCode") != null)
            { 
                PostalCode = HttpContext.Session.GetString("PostalCode");
                HasUserSelectedAddress = true;
                GetAddressesForPostalCode();
                FillCitizenAddressTable(addressCode);
            }          
        }

        public void OnPostVerifyAddress()
        {

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
                    addressinfo = JsonConvert.DeserializeObject<AddressesMain>(response);                  
                    HasUserEnteredPostalCcode = true;                  
                }
                catch
                {

                }
            }
        }

        public void FillCitizenAddressTable(int adddressCode)
        {
            Item addressData =  addressinfo.data.items.ToList().First(address => address.code == adddressCode);

            addressSummary = new AddressSummary();
            addressSummary.Street = addressData.name;
            addressSummary.Parish = addressinfo.data.parish.name;
            addressSummary.City = addressinfo.data.district.name;

        }        
    }
}
