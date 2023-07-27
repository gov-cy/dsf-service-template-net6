namespace DSF.AspNetCore.Web.Template.Data.Validations;

using DSF.AspNetCore.Web.Template.Data.Models;
using DSF.Localization;
using FluentValidation;

public class UserSatisfactionValidation : AbstractValidator<UserSatisfactionViewModel>
{
    private readonly IResourceViewLocalizer _localizer;

    public UserSatisfactionValidation(IResourceViewLocalizer localizer)
    {
        _localizer = localizer;

        RuleFor(a => a.SatisfactionSelection)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(_localizer["user-satisfaction.SatisfactionList.required"])
            .NotNull().WithMessage(_localizer["user-satisfaction.SatisfactionList.required"]);

        When(r => !string.IsNullOrEmpty(r.HowCouldWeImprove), () =>
        {
            RuleFor(r => r.HowCouldWeImprove)
                .Cascade(CascadeMode.Stop)
                .Length(1, 300).WithMessage(_localizer["user-satisfaction.Suggestion.length"]);
        });
    }
}
