namespace dsf_service_template_net6.Services.Model
{
    public enum SectionType
    {
        SelectionAndInput,
        InputOnly
    }
    public class SectionInfo
    {
        public string Name=string.Empty;
        public SectionType Type= SectionType.SelectionAndInput;
        public List<string> pages = new List<string>();
        public int SectionOrder=0;
    }
}
