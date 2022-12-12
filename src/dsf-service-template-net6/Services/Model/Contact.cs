namespace dsf_service_template_net6.Services.Model
{

    public class ContactInfoResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; } = "";
        public ContactInfo ?data { get; set; }
        public bool succeeded { get; set; }
        public string informationMessage { get; set; } = "";
    }

    public class ContactInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string MobileTelephone { get; set; } = "";
    }
}
