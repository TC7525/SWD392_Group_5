using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.User.Account.Request
{
    public class AccountBase
    {
        [TrimString(true)]
        [MaxLength(30)]
        public string? employee_code { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [TrimString(true)]
        [MaxLength(100)]
        public string full_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [TrimString(true)]
        [MaxLength(100)]
        public string email { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(20)]
        public string? phone { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống.")]
        public string role_id { get; set; } = string.Empty;

        public List<string> warehouse_ids { get; set; } = new();

        public eStatus_Account status { get; set; } = eStatus_Account.Active;
    }

    public class AccountCreating : AccountBase
    {
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string password { get; set; } = string.Empty;
    }

    public class AccountUpdating : AccountBase
    {
        [Required(ErrorMessage = "Id người dùng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class AccountStatusUpdating
    {
        [Required(ErrorMessage = "Id người dùng không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required]
        public eStatus_Account status { get; set; }
    }

    public class AccountResetPasswordRequest
    {
        [Required(ErrorMessage = "Id người dùng không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string new_password { get; set; } = string.Empty;
    }

    public class AccountTableFilter : TableFilterBase
    {
        public string? role_id { get; set; }
        public string? warehouse_id { get; set; }
        public eStatus_Account? status { get; set; }
    }
}
