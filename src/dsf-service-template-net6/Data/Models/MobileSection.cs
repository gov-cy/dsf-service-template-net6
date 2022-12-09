namespace dsf_service_template_net6.Data.Models
{
   
    public class MobileSection:ModelBase
    {
        public string mobile { get; set; } = "";
        public bool use_from_civil { get; set; }
        public bool use_other { get; set; }
    }
}
