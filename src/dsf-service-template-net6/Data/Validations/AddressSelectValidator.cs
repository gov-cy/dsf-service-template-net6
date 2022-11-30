using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class AddressSelectValidator : AbstractValidator<AddressSelect>
    {
        IResourceViewlocalizer _Localizer;
        string AddressNotFoundMsg = string.Empty;
        string AddressNoSelectionMsg = string.Empty;
        public AddressSelectValidator(IResourceViewlocalizer localizer)
        {
            _Localizer = localizer;
             RuleFor(x => x.use_from_civil)
                .Equal(true).When(x => x.use_other.Equals(false))
                .WithMessage(_Localizer["address-selection.require_check"]);
        }

    }
}
