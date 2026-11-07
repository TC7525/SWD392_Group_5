using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ewms_common.Attributes
{
    public class TrimStringAttribute : ValidationAttribute
    {
        private readonly bool _removeMultipleSpaces;

        public TrimStringAttribute(bool removeMultipleSpaces = false)
        {
            _removeMultipleSpaces = removeMultipleSpaces;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext ctx)
        {
            if (value is string stringValue)
            {
                var trimmedValue = stringValue.Trim();
                if (_removeMultipleSpaces)
                {
                    trimmedValue = Regex.Replace(trimmedValue, @"\s+", " ");
                }

                var property = ctx.ObjectType.GetProperty(ctx.MemberName!);
                if (property != null)
                {
                    property.SetValue(ctx.ObjectInstance, trimmedValue);
                }
            }
            return ValidationResult.Success!;
        }
    }
}
