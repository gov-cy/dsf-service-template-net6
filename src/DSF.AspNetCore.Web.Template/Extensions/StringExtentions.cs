namespace Dsf.Service.Template.Extensions
{
    public static class StringExtentions
    {
        public static string FormatMobile(this string mobile)
        {
            return mobile.Trim().StartsWith("00") ? $"+{mobile.Substring(2)}" : mobile;
        }
    }
}
