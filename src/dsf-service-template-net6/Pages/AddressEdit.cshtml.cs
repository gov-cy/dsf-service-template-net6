
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

        [BindProperty]
        [Required(ErrorMessage ="�������� ������������� ��")]
        
        public string PostalCode { get; set; }
                  
        public bool HasUserEnteredPostalCcode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;
        public AddressSummary addressSummary { get; set;}      
        public AddressesMain? addressinfo { get; set; }

        public void OnGet()
        {
          
        }

        public void OnPost()
        {
          
        }
       
        public void OnPostView()
        {

            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("PostalCode", PostalCode);
                GetAddressesForPostalCode();
            }
        }

        public void OnPostSelectAddressFromDropDown(int addressCode)
        {
            if (HttpContext.Session.GetString("PostalCode") != null)
            { 
                PostalCode = HttpContext.Session.GetString("PostalCode");
                HasUserSelectedAddress = true;
                GetAddressesForPostalCode();
                FillCitizenAddressTable(addressCode);
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
