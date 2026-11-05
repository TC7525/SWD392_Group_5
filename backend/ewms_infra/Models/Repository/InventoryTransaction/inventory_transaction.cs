using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Product;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_infra.Models.Repository.StockTransfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.InventoryTransaction
{
    public class inventory_transaction_properties : WarehouseModel
    {
        [Comment("Id sản phẩm")]
        [MaxLength(36)]
        public string product_id { get; set; } = string.Empty;

        [Comment("Id người thực hiện giao dịch")]
        [MaxLength(36)]
        public string user_id { get; set; } = string.Empty;

        [Comment("Id dòng đơn mua hàng (nếu là giao dịch nhập mua)")]
        [MaxLength(36)]
        public string? purchase_order_item_id { get; set; }

        [Comment("Id dòng đơn xuất kho (nếu là giao dịch xuất bán)")]
        [MaxLength(36)]
        public string? outbound_order_detail_id { get; set; }

        [Comment("Id phiếu chuyển kho (nếu là giao dịch chuyển kho)")]
        [MaxLength(36)]
        public string? stock_transfer_id { get; set; }

        [Comment("Loại giao dịch kho")]
        [EnumDataType(typeof(eType_InventoryTransaction))]
        public eType_InventoryTransaction transaction_type { get; set; }

        [Comment("Số lượng biến động (+ nhập / - xuất)")]
        public int quantity { get; set; } = 0;

        [Comment("Thời gian giao dịch")]
        public DateTime transaction_date { get; set; } = DateTime.Now;

        [Comment("Ghi chú giao dịch")]
        public string? note { get; set; }
    }

    [Comment("Bảng lịch sử biến động tồn kho")]
    [Table("inventory_transactions")]
    public class inventory_transaction : inventory_transaction_properties
    {
        [ForeignKey(nameof(product_id))]
        public virtual product? product { get; set; }

        [ForeignKey(nameof(warehouse_id))]
        public virtual warehouse? warehouse { get; set; }

        [ForeignKey(nameof(user_id))]
        public virtual user? user { get; set; }

        [ForeignKey(nameof(purchase_order_item_id))]
        public virtual purchase_order_item? purchase_order_item { get; set; }

        [ForeignKey(nameof(outbound_order_detail_id))]
        public virtual outbound_order_detail? outbound_order_detail { get; set; }

        [ForeignKey(nameof(stock_transfer_id))]
        public virtual stock_transfer? stock_transfer { get; set; }
    }

    public class inventory_transaction_configuration : IEntityTypeConfiguration<inventory_transaction>
    {
        public void Configure(EntityTypeBuilder<inventory_transaction> builder)
        {
            builder.HasIndex(e => e.product_id);
            builder.HasIndex(e => e.warehouse_id);
            builder.HasIndex(e => e.user_id);
            builder.HasIndex(e => e.transaction_type);
            builder.HasIndex(e => e.transaction_date);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.product)
                .WithMany(p => p.inventory_transactions)
                .HasForeignKey(e => e.product_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.warehouse)
                .WithMany(w => w.inventory_transactions)
                .HasForeignKey(e => e.warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.user)
                .WithMany(u => u.inventory_transactions)
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.purchase_order_item)
                .WithMany(poi => poi.inventory_transactions)
                .HasForeignKey(e => e.purchase_order_item_id)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.outbound_order_detail)
                .WithMany(ood => ood.inventory_transactions)
                .HasForeignKey(e => e.outbound_order_detail_id)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.stock_transfer)
                .WithMany(st => st.inventory_transactions)
                .HasForeignKey(e => e.stock_transfer_id)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
