using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Permission
{
    public class role_permission_properties : ExtendModel
    {
        [Comment("Id vai trò")]
        [MaxLength(36)]
        public string role_id { get; set; } = string.Empty;

        [Comment("Id quyền")]
        [MaxLength(36)]
        public string permission_id { get; set; } = string.Empty;
    }

    [Comment("Bảng phân quyền cho vai trò")]
    [Table("role_permissions")]
    public class role_permission : role_permission_properties
    {
        [ForeignKey(nameof(role_id))]
        public virtual role? role { get; set; }

        [ForeignKey(nameof(permission_id))]
        public virtual permission? permission { get; set; }
    }

    public class role_permission_configuration : IEntityTypeConfiguration<role_permission>
    {
        public void Configure(EntityTypeBuilder<role_permission> builder)
        {
            builder.HasIndex(e => new { e.role_id, e.permission_id }).IsUnique();
            builder.HasIndex(e => e.role_id);
            builder.HasIndex(e => e.permission_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.role)
                .WithMany(r => r.role_permissions)
                .HasForeignKey(e => e.role_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.permission)
                .WithMany(p => p.role_permissions)
                .HasForeignKey(e => e.permission_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
