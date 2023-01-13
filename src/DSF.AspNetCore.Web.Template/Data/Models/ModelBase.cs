namespace Dsf.Service.Template.Data.Models
{
    public enum ValidationMode
    {
        Select,
        Edit
    }
    public class ModelBase
    {
        public ValidationMode ValidationMode { get; set; }
    }
}
