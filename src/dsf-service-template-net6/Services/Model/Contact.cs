namespace dsf_service_template_net6.Services.Model
{

    public class ContactInfoResponse :BaseResponse<ContactInfo>
    {
        
    }

    public class ContactInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string MobileTelephone { get; set; } = "";
    }
}
