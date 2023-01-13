using DSF.MOI.CitizenData.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace DSF.Authentication
{
    class CyLoginSpecification : ICyLoginSpecification
    {
        public void SetClaims(ClaimActionCollection claimActions)
        {
            //Map custom fields
            //CY Login Specifications v2.1
            claimActions.MapJsonKey("unique_identifier", "unique_identifier");
            claimActions.MapJsonKey("legal_unique_identifier", "legal_unique_identifier");
            claimActions.MapJsonKey("legal_main_profile", "legal_main_profile");
            //EIDAS
            claimActions.MapJsonKey("given_name", "given_name");
            claimActions.MapJsonKey("family_name", "family_name");
            claimActions.MapJsonKey("birthdate", "birthdate");
        }
    }
}
