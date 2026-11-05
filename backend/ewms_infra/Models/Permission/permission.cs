using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Permission
{
    public class permission_properties : ExtendModel
    {
        [Comment("Mã quyền")]
        [MaxLength(80)]
        public string permission_code { get; set; } = string.Empty;

        [Comment("Tên quyền")]
        [MaxLength(150)]
        public string permission_name { get; set; } = string.Empty;

        [Comment("Phân hệ chức năng")]
        [MaxLength(50)]
        public string module { get; set; } = string.Empty;

        [Comment("Hành động")]
        [MaxLength(50)]
        public string action { get; set; } = string.Empty;

        [Comment("Mô tả quyền")]
        [MaxLength(255)]
        public string? description { get; set; }

        [Comment("Trạng thái quyền")]
        [EnumDataType(typeof(eStatus_Common))]
        public eStatus_Common status { get; set; } = eStatus_Common.Active;
    }

    [Comment("Bảng danh mục quyền hạn")]
    [Table("permissions")]
    public class permission : permission_properties
    {
        public virtual ICollection<role_permission> role_permissions { get; set; } = new List<role_permission>();
    }

    public class permission_configuration : IEntityTypeConfiguration<permission>
    {
        public void Configure(EntityTypeBuilder<permission> builder)
        {
            builder.HasIndex(e => e.permission_code).IsUnique();
            builder.HasIndex(e => e.module);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
