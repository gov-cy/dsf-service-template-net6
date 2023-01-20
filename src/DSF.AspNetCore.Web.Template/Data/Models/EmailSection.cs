namespace DSF.AspNetCore.Web.Template.Data.Models
{
    public class EmailSection:ModelBase
    {
        public string Email { get; set; } = "";
        public bool UseFromApi { get; set; }
        public bool UseOther { get; set; }
    }
}
