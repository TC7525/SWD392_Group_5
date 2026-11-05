using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Product;
using ewms_infra.Models.Repository.InventoryTransaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.OutboundOrder
{
    public class outbound_order_detail_properties : ExtendModel
    {
        [Comment("Id đơn xuất kho")]
        [MaxLength(36)]
        public string order_id { get; set; } = string.Empty;

        [Comment("Id sản phẩm")]
        [MaxLength(36)]
        public string product_id { get; set; } = string.Empty;

        [Comment("Số lượng yêu cầu xuất")]
        public int quantity { get; set; } = 0;

        [Comment("Số lượng đã thực xuất")]
        public int issued_quantity { get; set; } = 0;

        [Comment("Đơn giá bán")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal unit_price { get; set; } = 0;
    }

    [Comment("Bảng chi tiết đơn xuất kho")]
    [Table("outbound_order_details")]
    public class outbound_order_detail : outbound_order_detail_properties
    {
        [ForeignKey(nameof(order_id))]
        public virtual outbound_order? outbound_order { get; set; }

        [ForeignKey(nameof(product_id))]
        public virtual product? product { get; set; }

        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class outbound_order_detail_configuration : IEntityTypeConfiguration<outbound_order_detail>
    {
        public void Configure(EntityTypeBuilder<outbound_order_detail> builder)
        {
            builder.HasIndex(e => new { e.order_id, e.product_id }).IsUnique();
            builder.HasIndex(e => e.order_id);
            builder.HasIndex(e => e.product_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.outbound_order)
                .WithMany(o => o.outbound_order_details)
                .HasForeignKey(e => e.order_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.product)
                .WithMany(p => p.outbound_order_details)
                .HasForeignKey(e => e.product_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
