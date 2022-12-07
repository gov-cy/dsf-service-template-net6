using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
namespace dsf_service_template_net6.Data.Validations
{
    public class cMobileEditValidator: AbstractValidator<MobileEdit>
    {
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";
        string mobReq = string.Empty;
        string mobAlreadyExists = string.Empty;
        string mobValid= string.Empty;
        IResourceViewlocalizer _Localizer;
      public cMobileEditValidator(IResourceViewlocalizer localizer)
      {
            
            _Localizer = localizer;
            mobReq = _Localizer["set-mobile.require_check"];
            mobAlreadyExists= _Localizer["set-mobile.exist_check"];
            mobValid = _Localizer["set-mobile.format_check"];

            RuleFor(p => p.mobile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(mobReq)
                .NotEqual(x => x.prev_mobile).WithMessage(mobAlreadyExists)
                .Matches(Expression).WithMessage(mobValid); 
        }

    }
}
