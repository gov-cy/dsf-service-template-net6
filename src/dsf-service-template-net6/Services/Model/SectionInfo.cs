namespace dsf_service_template_net6.Services.Model
{
    public enum SectionType
    {
        SelectionAndInput,
        InputOnly
    }
    public class SectionInfo
    {
        public string Section=string.Empty;
        public SectionType PageType= SectionType.SelectionAndInput;
        public int SectionOrder=0;
    }
}
