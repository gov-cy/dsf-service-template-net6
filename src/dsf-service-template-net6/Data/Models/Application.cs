namespace dsf_service_template_net6.Data.Models
{

    public class ApplicationRequest
    {
        public string reference { get; set; }
        public Contactinfo contactInfo { get; set; }
    }

    public class Contactinfo
    {
        public string pin { get; set; }
        public string email { get; set; }
        public bool emailVerified { get; set; }
        public string mobile { get; set; }
        public bool mobileVerified { get; set; }
        public Addressinfo[] addressInfo { get; set; }
    }


    public class ApplicationResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public string data { get; set; }
        public bool succeeded { get; set; }
    }

}
