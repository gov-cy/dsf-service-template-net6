using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Resources
{
    public interface IResourceViewlocalizer
    {
        //Keys in all files must be unique
        public LocalizedString this[string key]
        {
            get;
        }
        LocalizedString GetPagesLocalizedString(string key) ;
        LocalizedString GetErrorLocalizedString(string key);
        LocalizedString GetCommonLocalizedString(string key);
    }
}
