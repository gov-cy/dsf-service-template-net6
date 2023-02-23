using DSF.AspNetCore.Web.Template.Data.Models;
using FluentValidation;
using DSF.AspNetCore.Web.Template.Services;
using DSF.Localization;

namespace DSF.AspNetCore.Web.Template.Data.Validations
{
    public class MobileValidator : AbstractValidator<MobileSection>
    {
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";

        private readonly string mobileNoSelectionMsg = string.Empty;
        private readonly string mobReq = string.Empty;
        private readonly string mobValid = string.Empty;

        private readonly ICommonApis _checker;
        private readonly IResourceViewLocalizer _Localizer;

        public MobileValidator(IResourceViewLocalizer localizer, ICommonApis commonApis)
        {
            _checker=commonApis;
            _Localizer = localizer;
            mobileNoSelectionMsg = _Localizer["mobile-selection.custom.required"];
            mobReq = _Localizer["set-mobile.mobile.required"];
            mobValid = _Localizer["sset-mobile.mobile.format"];
            When(p => p.ValidationMode.Equals(ValidationMode.Select), () =>
            {
                RuleFor(x => x.UseFromApi)
                .Equal(true)
                .When(x => x.UseOther.Equals(false))
                .WithErrorCode("mobile-selection.custom.required")
                .WithMessage(mobileNoSelectionMsg);
            });
            //Edit Mobile
            When(p => p.ValidationMode.Equals(ValidationMode.Edit), () =>
            {
                RuleFor(p => p.Mobile)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithErrorCode("set-mobile.mobile.required") //can be used for internal changes i.e. Unit test 
                 .WithMessage(mobReq)
                 .Matches("[0-9]{8,15}$")
                 .WithErrorCode("set-mobile.mobile.format")
                 .WithMessage(mobValid)
                 .Must(_checker.IsMobileValid)
                 .WithErrorCode("set-mobile.mobile.format")
                 .WithMessage(mobValid); 
            });
        }
    }    
}
