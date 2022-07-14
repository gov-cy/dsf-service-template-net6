using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dsf_service_template_net6.Extensions
{
    public static class ModelExtentions
    {

        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string className = "")
        {
            if (!result.IsValid)
            {
                //Clear errors before
                foreach (var modelValue in modelState.Values)
                {
                    modelValue.Errors.Clear();
                }
                foreach (var error in result.Errors)
                {
                    if (string.IsNullOrEmpty(className))
                    {
                        modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    else
                    {
                        modelState.AddModelError(className + "." + error.PropertyName, error.ErrorMessage);
                    }

                }
            }
        }
    }
    
}
