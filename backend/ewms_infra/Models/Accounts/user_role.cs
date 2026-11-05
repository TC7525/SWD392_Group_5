using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Accounts
{
    public class user_role_properties : ExtendModel
    {
        [Comment("Id người dùng")]
        [MaxLength(36)]
        public string user_id { get; set; } = string.Empty;

        [Comment("Id vai trò")]
        [MaxLength(36)]
        public string role_id { get; set; } = string.Empty;

        [Comment("Thời gian gán vai trò")]
        public DateTime assigned_at { get; set; } = DateTime.Now;

        [Comment("Người gán vai trò")]
        [MaxLength(36)]
        public string? assigned_by { get; set; }
    }

    [Comment("Bảng phân quyền vai trò cho người dùng")]
    [Table("user_roles")]
    public class user_role : user_role_properties
    {
        [ForeignKey(nameof(user_id))]
        public virtual user? user { get; set; }

        [ForeignKey(nameof(role_id))]
        public virtual role? role { get; set; }
    }

    public class user_role_configuration : IEntityTypeConfiguration<user_role>
    {
        public void Configure(EntityTypeBuilder<user_role> builder)
        {
            builder.HasIndex(e => new { e.user_id, e.role_id }).IsUnique();
            builder.HasIndex(e => e.user_id);
            builder.HasIndex(e => e.role_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.user)
                .WithMany(u => u.user_roles)
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.role)
                .WithMany(r => r.user_roles)
                .HasForeignKey(e => e.role_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
