using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ewms_common.Configurations
{
    public class LocalDateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var dateTime))
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                bindingContext.Result = ModelBindingResult.Success(dateTime);
            }
            else if (string.IsNullOrEmpty(value) && bindingContext.ModelType == typeof(DateTime?))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }
    }

    public class LocalDateTimeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
            {
                return new LocalDateTimeModelBinder();
            }
            return null!;
        }
    }
}
