namespace DSF.AspNetCore.Web.Template.Data.Models
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
