using System.ComponentModel.DataAnnotations;

namespace ewms_common.Attributes
{
    public class RemoveDuplicateStringsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext ctx)
        {
            if (value is IList<string> stringValues)
            {
                var filterdStrings = stringValues.Distinct().ToList();
                var property = ctx.ObjectType.GetProperty(ctx.MemberName!);
                if (property != null)
                {
                    property.SetValue(ctx.ObjectInstance, filterdStrings);
                }
            }
            return ValidationResult.Success!;
        }
    }
}
