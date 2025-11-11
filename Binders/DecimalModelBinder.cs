using System;
using System.Globalization;
using System.Web.Mvc;

namespace ValiModern.Binders
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == null)
            {
                return null;
            }

            var value = valueProviderResult.AttemptedValue;
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            // Replace comma with a period for cultures that use it as a decimal separator
            value = value.Replace(",", ".");

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid decimal number.");
            return null;
        }
    }
}
