namespace DSF.AspNetCore.Web.Template.Services.UserSatisfaction;

using DSF.AspNetCore.Web.Template.Services.Model;
using DSF.AspNetCore.Web.Template.Services.UserSatisfaction.Data;

public interface IUserSatisfactionService
{
    public BaseResponse<string> SubmitUserSatisfaction(UserSatisfactionServiceRequest request);
}
