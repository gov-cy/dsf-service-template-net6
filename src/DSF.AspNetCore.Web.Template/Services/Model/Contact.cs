namespace Dsf.Service.Template.Services.Model
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
