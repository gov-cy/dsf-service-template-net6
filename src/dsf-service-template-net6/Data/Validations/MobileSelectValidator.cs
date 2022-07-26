﻿using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class MobileSelectValidator : AbstractValidator<MobileSelect>
    {
        IStringLocalizer _Localizer;
        string MobileNumNotFoundMsg = string.Empty;
        string MobileNoSelectionMsg = string.Empty;
        public MobileSelectValidator(IStringLocalizer localizer)
        {

            _Localizer = localizer;
            MobileNumNotFoundMsg = _Localizer["MobileNotFound"];
            MobileNoSelectionMsg = _Localizer["MobileSelection"];
            RuleFor(x => x.mobile).NotEmpty().NotNull().WithMessage(MobileNumNotFoundMsg);
            RuleFor(x => x.use_from_civil).Equal(true).When(x => x.use_other.Equals(false)).WithMessage(MobileNoSelectionMsg);
        }

    }
}