using DSF.AspNetCore.Web.Template.Data.Models;
using FluentValidation;
using DSF.AspNetCore.Web.Template.Services;
using DSF.Resources;

namespace DSF.AspNetCore.Web.Template.Data.Validations
{
    public class EmailValidator : AbstractValidator<EmailSection>
    {
        readonly ICommonApis _checker;
        readonly IResourceViewLocalizer _Localizer;
        readonly string emailNoSelectionMsg = string.Empty;
        readonly string emailMessage = string.Empty;
        public EmailValidator(IResourceViewLocalizer localizer, ICommonApis checker)
        {
            _checker = checker;
            _Localizer = localizer;
            emailNoSelectionMsg = _Localizer["email-selection.require_check"];
            emailMessage = _Localizer["set-email.require_check"];

            When(p => p.ValidationMode.Equals(ValidationMode.Select), () =>
            {
                RuleFor(x => x.UseFromApi)
                    .Equal(true)
                    .When(x => x.UseOther.Equals(false))
                    .WithErrorCode("email-selection.require_check")
                    .WithMessage(emailNoSelectionMsg);
            });
            //Edit page
            When(p => p.ValidationMode.Equals(ValidationMode.Edit), () =>
            {
                RuleFor(p => p.Email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode("set-email.require_check")
                    .WithMessage(emailMessage)
                    .Must(_checker.IsEmailValid)
                    .WithErrorCode("set-email.require_check")
                    .WithMessage(emailMessage);
            });
        }
    }
}
