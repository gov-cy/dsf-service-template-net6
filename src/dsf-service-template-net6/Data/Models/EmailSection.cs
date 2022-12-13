namespace Dsf.Service.Template.Data.Models
{
 
    public class EmailSection:ModelBase
    {
        public string email { get; set; } = "";
        public bool use_from_api { get; set; }
        public bool use_other { get; set; }
        
    }
}
