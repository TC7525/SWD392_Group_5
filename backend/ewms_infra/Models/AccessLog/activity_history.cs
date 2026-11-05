using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.AccessLog
{
    public class activity_history_properties : ExtendModel
    {
        [Comment("Id người dùng thực hiện")]
        [MaxLength(36)]
        public string? user_id { get; set; }

        [Comment("Phân hệ thực hiện")]
        [MaxLength(50)]
        public string module { get; set; } = string.Empty;

        [Comment("Hành động thực hiện")]
        [MaxLength(50)]
        public string action { get; set; } = string.Empty;

        [Comment("Loại đối tượng tác động")]
        [MaxLength(50)]
        public string? entity_type { get; set; }

        [Comment("Id đối tượng tác động")]
        [MaxLength(36)]
        public string? entity_id { get; set; }

        [Comment("Mô tả chi tiết hoạt động")]
        public string? description { get; set; }

        [Comment("Địa chỉ IP")]
        [MaxLength(45)]
        public string? ip_address { get; set; }
    }

    [Comment("Bảng lịch sử hoạt động hệ thống")]
    [Table("activity_histories")]
    public class activity_history : activity_history_properties
    {
        [ForeignKey(nameof(user_id))]
        public virtual user? user { get; set; }
    }

    public class activity_history_configuration : IEntityTypeConfiguration<activity_history>
    {
        public void Configure(EntityTypeBuilder<activity_history> builder)
        {
            builder.HasIndex(e => e.user_id);
            builder.HasIndex(e => e.module);
            builder.HasIndex(e => e.created_on);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.user)
                .WithMany(u => u.activity_histories)
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
