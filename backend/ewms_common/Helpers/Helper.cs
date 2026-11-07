using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using ewms_common.Authentications;
using ewms_common.Wrappers;

namespace ewms_common.Helpers
{
    public static class Helper
    {
        public static IList<string> GetAllCreatedByAndModifiedBy<T>(IList<T> objects) where T : class
        {
            var result = new List<string>();
            if (objects == null || !objects.Any()) return result;

            foreach (var obj in objects)
            {
                var createdByProperty = typeof(T).GetProperty("created_by");
                var modifiedByProperty = typeof(T).GetProperty("modified_by");

                if (createdByProperty != null)
                {
                    var createdByValue = createdByProperty.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(createdByValue))
                        result.Add(createdByValue);
                }

                if (modifiedByProperty != null)
                {
                    var modifiedByValue = modifiedByProperty.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(modifiedByValue))
                        result.Add(modifiedByValue);
                }
            }

            return result.Distinct().ToList();
        }

        public static List<ImportDataError> ValidateAttribute<T>(T obj)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(obj!);
            bool isValid = Validator.TryValidateObject(obj!, validationContext, validationResults, true);

            var errors = new List<ImportDataError>();
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var name in validationResult.MemberNames)
                    {
                        if (validationResult.ErrorMessage != null)
                        {
                            errors.Add(new ImportDataError
                            {
                                name = name,
                                message = validationResult.ErrorMessage,
                            });
                        }
                    }
                }
            }
            return errors;
        }

        public static CurrentUser GetCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            var user = new CurrentUser();
            if (httpContextAccessor.HttpContext != null)
            {
                var User = httpContextAccessor.HttpContext.User;
                user.user_id = User.FindFirst(CustomJwtClaimType.UserId)?.Value ?? "";
                user.phone = User.FindFirst(CustomJwtClaimType.Phone)?.Value ?? "";
                user.email = User.FindFirst(CustomJwtClaimType.Email)?.Value ?? "";
                user.employee_code = User.FindFirst(CustomJwtClaimType.EmployeeCode)?.Value ?? "";
                user.fullname = User.FindFirst(CustomJwtClaimType.Fullname)?.Value ?? "";
                user.role = User.FindFirst(CustomJwtClaimType.Role)?.Value ?? "";
                user.role_id = User.FindFirst(CustomJwtClaimType.RoleId)?.Value ?? "";
                user.warehouse_id = User.FindFirst(CustomJwtClaimType.WarehouseId)?.Value ?? "";

                var forwardedFor = httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    user.ip_address = forwardedFor.Split(',').First().Trim();
                }
                else
                {
                    user.ip_address = httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                }
            }
            return user;
        }

        public static string GenerateNextCode(List<string> existingCodes, string prefix, int digitNumber = 5)
        {
            int maxNum = 0;
            foreach (var code in existingCodes)
            {
                if (string.IsNullOrWhiteSpace(code)) continue;
                var trimmed = code.Trim();
                if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                var numberPart = trimmed.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var num) && num > maxNum)
                {
                    maxNum = num;
                }
            }
            return $"{prefix}{(maxNum + 1).ToString().PadLeft(digitNumber, '0')}";
        }
    }
}
