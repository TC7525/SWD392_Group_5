using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository
{
    public class user_warehouse_properties : ExtendModel
    {
        [Comment("Id người dùng")]
        [MaxLength(36)]
        public string user_id { get; set; } = string.Empty;

        [Comment("Id kho hàng")]
        [MaxLength(36)]
        public string warehouse_id { get; set; } = string.Empty;

        [Comment("Thời gian phân công kho")]
        public DateTime assigned_at { get; set; } = DateTime.Now;
    }

    [Comment("Bảng phân công kho cho nhân sự")]
    [Table("user_warehouses")]
    public class user_warehouse : user_warehouse_properties
    {
        [ForeignKey(nameof(user_id))]
        public virtual user? user { get; set; }

        [ForeignKey(nameof(warehouse_id))]
        public virtual warehouse? warehouse { get; set; }
    }

    public class user_warehouse_configuration : IEntityTypeConfiguration<user_warehouse>
    {
        public void Configure(EntityTypeBuilder<user_warehouse> builder)
        {
            builder.HasIndex(e => new { e.user_id, e.warehouse_id }).IsUnique();
            builder.HasIndex(e => e.user_id);
            builder.HasIndex(e => e.warehouse_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.user)
                .WithMany(u => u.user_warehouses)
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.warehouse)
                .WithMany(w => w.user_warehouses)
                .HasForeignKey(e => e.warehouse_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
