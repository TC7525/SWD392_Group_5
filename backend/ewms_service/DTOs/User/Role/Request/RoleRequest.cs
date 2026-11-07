using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.User.Role.Request
{
    public class RoleBase
    {
        [Required(ErrorMessage = "Mã vai trò không được để trống.")]
        [TrimString(true)]
        [MaxLength(50)]
        public string role_code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [TrimString(true)]
        [MaxLength(100)]
        public string role_name { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(255)]
        public string? description { get; set; }

        public eStatus_Common status { get; set; } = eStatus_Common.Active;

        public List<string> permission_ids { get; set; } = new();
    }

    public class RoleCreating : RoleBase
    {
    }

    public class RoleUpdating : RoleBase
    {
        [Required(ErrorMessage = "Id vai trò không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class RoleTableFilter : TableFilterBase
    {
        public eStatus_Common? status { get; set; }
    }
}
