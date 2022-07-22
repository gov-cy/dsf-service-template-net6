namespace dsf_service_template_net6.Data.Models
{
    public class AddressesMain
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