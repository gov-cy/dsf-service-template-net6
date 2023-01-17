namespace DSF.MOI.CitizenData.Web.Configuration
{
    public class OIDCSettings
    {
        public string? Authority { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Scopes { get; set; }        
        public string? RedirectUri { get; set; }
        /// <summary>
        /// Set the url to the starting point of your application e.g. "/Account/LogIn"
        /// </summary>
        public string? LoginUrl { get; set; }
        public string? SignedOutRedirectUri { get; set; }
    }
}
