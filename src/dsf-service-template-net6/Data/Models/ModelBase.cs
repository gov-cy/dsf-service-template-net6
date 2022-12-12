namespace dsf_service_template_net6.Data.Models
{
    public enum ValidationMode
    {
        Select,
        Edit

    }
    public class ModelBase
    {
        public ValidationMode validation_mode { get; set; }
    }
}
