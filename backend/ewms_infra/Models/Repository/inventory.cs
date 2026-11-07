using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository
{
    public class inventory_properties : WarehouseModel
    {
        [Comment("Id sản phẩm")]
        [MaxLength(36)]
        public string product_id { get; set; } = string.Empty;

        [Comment("Số lượng tồn kho hiện tại")]
        public int quantity { get; set; } = 0;

        [Comment("Thời gian cập nhật tồn kho gần nhất")]
        public DateTime last_updated_at { get; set; } = DateTime.Now;
    }

    [Comment("Bảng tồn kho theo từng kho và sản phẩm")]
    [Table("inventories")]
    public class inventory : inventory_properties
    {
        [ForeignKey(nameof(product_id))]
        public virtual product? product { get; set; }

        [ForeignKey(nameof(warehouse_id))]
        public virtual warehouse? warehouse { get; set; }
    }

    public class inventory_configuration : IEntityTypeConfiguration<inventory>
    {
        public void Configure(EntityTypeBuilder<inventory> builder)
        {
            builder.HasIndex(e => new { e.product_id, e.warehouse_id }).IsUnique();
            builder.HasIndex(e => e.warehouse_id);
            builder.HasIndex(e => e.product_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.product)
                .WithMany(p => p.inventories)
                .HasForeignKey(e => e.product_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.warehouse)
                .WithMany(w => w.inventories)
                .HasForeignKey(e => e.warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
