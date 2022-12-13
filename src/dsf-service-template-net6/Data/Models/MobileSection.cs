namespace Dsf.Service.Template.Data.Models
{
   
    public class MobileSection:ModelBase
    {
        public string mobile { get; set; } = "";
        public bool use_from_api { get; set; }
        public bool use_other { get; set; }
    }
}
