using Microsoft.Extensions.Localization;
using System.Reflection;
namespace dsf_service_template_net6.Resources
{
    public class ResourceViewlocalizer : IResourceViewlocalizer

    {
        private readonly IStringLocalizer localizerPages;
        private readonly IStringLocalizer localizerError;
        private readonly IStringLocalizer localizerCommon;
        public ResourceViewlocalizer(IStringLocalizerFactory factory)
        {
            var type = typeof(PageResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName!);
            var type1 = typeof(ErrorResource);
            var assemblyName1 = new AssemblyName(type1.GetTypeInfo().Assembly.FullName!);
            var type2 = typeof(CommonResource);
            var assemblyName2 = new AssemblyName(type2.GetTypeInfo().Assembly.FullName!);
            localizerPages = factory.Create("PageResource", assemblyName.Name!);
            localizerError = factory.Create("ErrorResource", assemblyName1.Name!);
            localizerCommon = factory.Create("CommonResource", assemblyName2.Name!);
        }
        public LocalizedString this[string key] => (!this.localizerCommon[key].ResourceNotFound) ? this.localizerCommon[key] : (!this.localizerError[key].ResourceNotFound) ? this.localizerError[key] : this.localizerPages[key] ;

        public LocalizedString GetErrorLocalizedString(string key)
        {
            return this.localizerError[key];
        }
        public LocalizedString GetPagesLocalizedString(string key)
        {
            return this.localizerPages[key]; 
        }
        public LocalizedString GetCommonLocalizedString(string key)
        {
            return this.localizerCommon[key];
        }
    }
}
