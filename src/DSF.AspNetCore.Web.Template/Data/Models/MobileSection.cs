namespace Dsf.Service.Template.Data.Models
{
    public class MobileSection:ModelBase
    {
        public string Mobile { get; set; } = "";
        public bool UseFromApi { get; set; }
        public bool UseOther { get; set; }
    }
}
