namespace dsf_service_template_net6.Services.Model
{

    public class ContactInfoResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public ContactInfo data { get; set; }
        public bool succeeded { get; set; }
        public string informationMessage { get; set; }
    }

    public class ContactInfo
    {
        public int id { get; set; }
        public string email { get; set; }
        public string mobileTelephone { get; set; }
    }
}
