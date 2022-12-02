namespace dsf_service_template_net6.Data.Models
{

    public class CitizenDataResponse
    {
        public int errorCode { get; set; }
        public object errorMessage { get; set; }
        public CitizenDataDetails data { get; set; }
        public bool succeeded { get; set; }
    }

    public class CitizenDataDetails
    {
        public string fullName { get; set; }
        public string dob { get; set; }
        public object dod { get; set; }
        public string pin { get; set; }
        public string email { get; set; }
        public bool emailVerified { get; set; }
        public string mobile { get; set; }
        public bool mobileVerified { get; set; }
        public Addressinfo[] addressInfo { get; set; }
    }

    public class Addressinfo
    {
        public string type { get; set; }
        public int postalCode { get; set; }
        public string language { get; set; }
        public Item item { get; set; }
        public Town town { get; set; }
        public Parish parish { get; set; }
        public District district { get; set; }
        public Country country { get; set; }
        public bool addressVerified { get; set; }
        public string addressText { get; set; }
    }

    public class Item
    {
        public Street street { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Street
    {
        public string apartmentNumber { get; set; }
        public string streetNumber { get; set; }
    }

    public class Town
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class Parish
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class District
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class Country
    {
        public int code { get; set; }
        public string name { get; set; }
    }
  

}
