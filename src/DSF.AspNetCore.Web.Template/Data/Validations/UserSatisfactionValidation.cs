namespace DSF.AspNetCore.Web.Template.Data.Validations;

using DSF.AspNetCore.Web.Template.Pages;
using DSF.Localization;
using FluentValidation;

public class UserSatisfactionValidation : AbstractValidator<UserSatisfaction>
{
    private readonly IResourceViewLocalizer _localizer;

    public UserSatisfactionValidation(IResourceViewLocalizer localizer)
    {
        _localizer = localizer;

        RuleFor(a => a.SatisfactionSelection)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(_localizer["user-satisfaction.SatisfactionList.required"])
            .NotNull().WithMessage(_localizer["user-satisfaction.SatisfactionList.required"]);
    }
}

