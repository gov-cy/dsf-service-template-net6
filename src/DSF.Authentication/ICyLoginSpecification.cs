using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace DSF.MOI.CitizenData.Web.Configuration
{
    public interface ICyLoginSpecification
    {
        public void SetClaims(ClaimActionCollection claimActions);
    }
}
