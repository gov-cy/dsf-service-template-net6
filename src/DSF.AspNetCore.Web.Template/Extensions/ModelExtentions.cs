using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DSF.AspNetCore.Web.Template.Extensions
{
    public static class ModelExtentions
    {
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string className = "")
        {
            if (result?.IsValid==false)
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
                        var propName = string.IsNullOrEmpty(error.PropertyName) ? error.ErrorMessage : error.PropertyName;
                        modelState.AddModelError(propName, error.ErrorMessage);
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
