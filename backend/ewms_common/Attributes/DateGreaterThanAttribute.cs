using System.ComponentModel.DataAnnotations;

namespace ewms_common.Attributes
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = (DateTime?)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);
            string[] displayNames = { validationContext.DisplayName };
            if (currentValue.HasValue && comparisonValue.HasValue && currentValue < comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} phải lớn hơn hoặc bằng {property.Name}", displayNames);
            }

            return ValidationResult.Success!;
        }
    }
}
