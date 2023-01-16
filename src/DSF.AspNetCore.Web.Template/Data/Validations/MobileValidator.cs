using DSF.AspNetCore.Web.Template.Data.Models;
using FluentValidation;
using DSF.AspNetCore.Web.Template.Services;
using DSF.Resources;

namespace DSF.AspNetCore.Web.Template.Data.Validations
{
    public class MobileValidator : AbstractValidator<MobileSection>
    {
        readonly ICommonApis _checker;
        readonly IResourceViewLocalizer _Localizer;
        string MobileNoSelectionMsg = string.Empty;
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";
        string mobReq = string.Empty;
        string mobValid = string.Empty;
        public MobileValidator(IResourceViewLocalizer localizer, ICommonApis commonApis)
        {
            _checker=commonApis;
            _Localizer = localizer;
            MobileNoSelectionMsg = _Localizer["mobile-selection.require_check"];
            mobReq = _Localizer["set-mobile.require_check"];
            mobValid = _Localizer["set-mobile.format_check"];
            When(p => p.ValidationMode.Equals(ValidationMode.Select), () =>
            {
                RuleFor(x => x.UseFromApi)
                .Equal(true)
                .When(x => x.UseOther.Equals(false))
                .WithErrorCode("mobile-selection.require_check")
                .WithMessage(MobileNoSelectionMsg);
            });
            //Edit Mobile
            When(p => p.ValidationMode.Equals(ValidationMode.Edit), () =>
            {
                RuleFor(p => p.Mobile)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithErrorCode("set-mobile.require_check") //can be used for internal changes i.e. Unit test 
                 .WithMessage(mobReq)
                 .Matches("[0-9]{8,15}$")
                 .WithErrorCode("set-mobile.format_check")
                 .WithMessage(mobValid)
                 .Must(_checker.IsMobileValid)
                 .WithErrorCode("set-mobile.format_check")
                 .WithMessage(mobValid); 
            });
        }
    }    
}
