using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Product;
using ewms_infra.Models.Repository.InventoryTransaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.PurchaseOrder
{
    public class purchase_order_item_properties : ExtendModel
    {
        [Comment("Id đơn mua hàng")]
        [MaxLength(36)]
        public string purchase_order_id { get; set; } = string.Empty;

        [Comment("Id sản phẩm")]
        [MaxLength(36)]
        public string product_id { get; set; } = string.Empty;

        [Comment("Số lượng đặt mua")]
        public int ordered_quantity { get; set; } = 0;

        [Comment("Số lượng đã thực nhập")]
        public int received_quantity { get; set; } = 0;

        [Comment("Đơn giá mua")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal unit_price { get; set; } = 0;
    }

    [Comment("Bảng chi tiết đơn mua hàng")]
    [Table("purchase_order_items")]
    public class purchase_order_item : purchase_order_item_properties
    {
        [ForeignKey(nameof(purchase_order_id))]
        public virtual purchase_order? purchase_order { get; set; }

        [ForeignKey(nameof(product_id))]
        public virtual product? product { get; set; }

        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class purchase_order_item_configuration : IEntityTypeConfiguration<purchase_order_item>
    {
        public void Configure(EntityTypeBuilder<purchase_order_item> builder)
        {
            builder.HasIndex(e => new { e.purchase_order_id, e.product_id }).IsUnique();
            builder.HasIndex(e => e.purchase_order_id);
            builder.HasIndex(e => e.product_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.purchase_order)
                .WithMany(po => po.purchase_order_items)
                .HasForeignKey(e => e.purchase_order_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.product)
                .WithMany(p => p.purchase_order_items)
                .HasForeignKey(e => e.product_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
