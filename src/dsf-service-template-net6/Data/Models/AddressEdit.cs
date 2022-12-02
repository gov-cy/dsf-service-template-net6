using Microsoft.AspNetCore.Mvc.Rendering;

namespace dsf_service_template_net6.Data.Models
{
    public class AddressEditViewModel
    {
        public string postalCode { get; set; }
        public List<SelectListItem> Addresses;
        public bool HasUserEnteredPostalCode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;
        public bool ShowErrorSummary { get; set; } = false;
        public bool NoResultsFound { get; set; } = false;
        public string ErrorDesc { get; set; } = string.Empty;
        public string SelectedAddress { get; set; }
        public string City { get; set; }
        public string Town { get; set; }
        public string Parish { get; set; }
        public string SelectedAddressName { get; set; }
        public string FlatNo { get; set; }
        public string StreetNo { get; set; }
    }
    public class AddressEdit
    {
        public int errorCode { get; set; }
        public object errorMessage { get; set; }
        public AddressData data { get; set; }
        public bool succeeded { get; set; }
    }

    public class AddressData
    {
        public int postalCode { get; set; }
        public string language { get; set; }
        public Item[] items { get; set; }
        public Town town { get; set; }
        public Parish parish { get; set; }
        public District district { get; set; }
        public Country country { get; set; }
    }

   
}