using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Permission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Accounts
{
    public class role_properties : ExtendModel
    {
        [Comment("Mã vai trò")]
        [MaxLength(50)]
        public string role_code { get; set; } = string.Empty;

        [Comment("Tên vai trò")]
        [MaxLength(100)]
        public string role_name { get; set; } = string.Empty;

        [Comment("Mô tả vai trò")]
        [MaxLength(255)]
        public string? description { get; set; }

        [Comment("Trạng thái vai trò")]
        [EnumDataType(typeof(eStatus_Common))]
        public eStatus_Common status { get; set; } = eStatus_Common.Active;
    }

    [Comment("Bảng vai trò người dùng")]
    [Table("roles")]
    public class role : role_properties
    {
        public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();
        public virtual ICollection<role_permission> role_permissions { get; set; } = new List<role_permission>();
    }

    public class role_configuration : IEntityTypeConfiguration<role>
    {
        public void Configure(EntityTypeBuilder<role> builder)
        {
            builder.HasIndex(e => e.role_code).IsUnique();
            builder.HasIndex(e => e.role_name).IsUnique();
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
