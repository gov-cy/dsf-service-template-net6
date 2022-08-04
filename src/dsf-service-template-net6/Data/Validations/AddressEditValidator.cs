using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class AddressEditValidator : AbstractValidator<AddressEditViewModel>
    {
        IStringLocalizer _Localizer;
        string AddressNotFoundMsg = string.Empty;
        string PostalRequiredMsg = string.Empty;
        public AddressEditValidator(IStringLocalizer localizer)
        {

            _Localizer = localizer;
            AddressNotFoundMsg = _Localizer["AddressNotFound"];
            PostalRequiredMsg = _Localizer["PostalRequired"];
            RuleFor(x => x.postalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(PostalRequiredMsg);
        }
    }
}
