// HomeOwners/Infrastructure/TimeSpanModelBinder.cs
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace HomeOwners.Infrastructure
{
    public class TimeSpanModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            try
            {
                // HTML time inputs produce strings like "13:45"
                // Try to parse with seconds
                if (TimeSpan.TryParse(value, out TimeSpan timeSpan))
                {
                    bindingContext.Result = ModelBindingResult.Success(timeSpan);
                    return Task.CompletedTask;
                }

                // Try to parse without seconds (HH:mm format)
                if (value.Length == 5 && value.Contains(':'))
                {
                    string[] parts = value.Split(':');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int hours) &&
                        int.TryParse(parts[1], out int minutes))
                    {
                        timeSpan = new TimeSpan(hours, minutes, 0);
                        bindingContext.Result = ModelBindingResult.Success(timeSpan);
                        return Task.CompletedTask;
                    }
                }

                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    $"Could not parse {value} as a valid time. Use HH:MM format.");
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exception.Message);
                return Task.CompletedTask;
            }
        }
    }

    public class TimeSpanModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(TimeSpan) || context.Metadata.ModelType == typeof(TimeSpan?))
                return new BinderTypeModelBinder(typeof(TimeSpanModelBinder));

            return null;
        }
    }
}