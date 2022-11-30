namespace dsf_service_template_net6.Services.Model
{
    public enum PageType
    {
        Selection,
        Input
    }
    public class PageInfo
    {
        public string Section=string.Empty;
        public PageType PageType= PageType.Selection;
        public string FormName = string.Empty;
        public int PageOrder=0;
    }
}
