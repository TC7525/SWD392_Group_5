using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.StockTransfer
{
    public class stock_transfer_item_properties : ExtendModel
    {
        [Comment("Id phiếu chuyển kho")]
        [MaxLength(36)]
        public string stock_transfer_id { get; set; } = string.Empty;

        [Comment("Id sản phẩm")]
        [MaxLength(36)]
        public string product_id { get; set; } = string.Empty;

        [Comment("Số lượng yêu cầu chuyển")]
        public int quantity { get; set; } = 0;

        [Comment("Số lượng đã xuất từ kho nguồn")]
        public int shipped_quantity { get; set; } = 0;

        [Comment("Số lượng đã nhận tại kho đích")]
        public int received_quantity { get; set; } = 0;

        [Comment("Ghi chú dòng sản phẩm")]
        public string? note { get; set; }
    }

    [Comment("Bảng chi tiết phiếu chuyển kho")]
    [Table("stock_transfer_items")]
    public class stock_transfer_item : stock_transfer_item_properties
    {
        [ForeignKey(nameof(stock_transfer_id))]
        public virtual stock_transfer? stock_transfer { get; set; }

        [ForeignKey(nameof(product_id))]
        public virtual product? product { get; set; }
    }

    public class stock_transfer_item_configuration : IEntityTypeConfiguration<stock_transfer_item>
    {
        public void Configure(EntityTypeBuilder<stock_transfer_item> builder)
        {
            builder.HasIndex(e => new { e.stock_transfer_id, e.product_id }).IsUnique();
            builder.HasIndex(e => e.stock_transfer_id);
            builder.HasIndex(e => e.product_id);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.stock_transfer)
                .WithMany(st => st.stock_transfer_items)
                .HasForeignKey(e => e.stock_transfer_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.product)
                .WithMany(p => p.stock_transfer_items)
                .HasForeignKey(e => e.product_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
