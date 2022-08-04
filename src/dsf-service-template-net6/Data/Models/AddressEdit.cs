using Microsoft.AspNetCore.Mvc.Rendering;

namespace dsf_service_template_net6.Data.Models
{
    public class AddressEditViewModel
    {
        public string postalCode { get; set; }
        public List<SelectListItem> Addresses;
        public bool HasUserEnteredPostalCode { get; set; } = false;
        public bool HasUserSelectedAddress { get; set; } = false;
        public string FormClassNoError { get; set; } = "govcy-form-control";
        public string FormClassWithError { get; set; } = "govcy-form-control govcy-form-control-error";
        public string PostalCodeFormClass { get; set; } = "govcy-form-control";
        public string PostalCodeTextboxCSS { get; set; }
        public string FlatNoTextboxCSS { get; set; }
        public string FlatNoTextboxCSSNoError { get; set; } = "govcy-text-input govcy-text-input-char_5";
        public string FlatNoTextboxCSSWithError { get; set; } = "govcy-text-input govcy-text-input-char_5 govcy-text-input-error";
        public string postalCodeTextboxCSS { get; set; }
        public string PostalCodeTextboxCSSNoError { get; set; } = "govcy-text-input govcy-text-input-char_4";
        public string PostalCodeTextboxCSSWithError { get; set; } = "govcy-text-input govcy-text-input-char_4 govcy-text-input-error";
        public string VerifyAddressFormClass { get; set; }
        public bool ShowErrorSummary { get; set; } = false; 
        public string ErrorDesc { get; set; } = string.Empty;
        public string SelectedAddress { get; set; }
        public string City { get; set; }
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