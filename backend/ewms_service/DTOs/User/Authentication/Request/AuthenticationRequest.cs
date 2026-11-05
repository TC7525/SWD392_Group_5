using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;

namespace ewms_service.DTOs.User.Authentication.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email hoặc Mã nhân viên không được để trống.")]
        [TrimString(true)]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string password { get; set; } = string.Empty;

        public string? warehouse_id { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
        public string old_password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string new_password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        public string confirm_password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [TrimString(true)]
        [MaxLength(100)]
        public string full_name { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(20)]
        public string? phone { get; set; }
    }

    public class SwitchWarehouseRequest
    {
        [Required(ErrorMessage = "Id kho hàng không được để trống.")]
        public string warehouse_id { get; set; } = string.Empty;
    }
}
