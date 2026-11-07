using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Contact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.PurchaseOrder
{
    public class purchase_order_properties : WarehouseModel
    {
        [Comment("Mã đơn mua hàng")]
        [MaxLength(40)]
        public string purchase_order_code { get; set; } = string.Empty;

        [Comment("Id nhà cung cấp")]
        [MaxLength(36)]
        public string supplier_id { get; set; } = string.Empty;

        [Comment("Id người phê duyệt")]
        [MaxLength(36)]
        public string? approved_by { get; set; }

        [Comment("Trạng thái đơn mua hàng")]
        [EnumDataType(typeof(eStatus_PurchaseOrder))]
        public eStatus_PurchaseOrder status { get; set; } = eStatus_PurchaseOrder.Pending;

        [Comment("Ngày đặt hàng")]
        public DateTime order_date { get; set; } = DateTime.Now;

        [Comment("Ngày dự kiến giao hàng")]
        public DateTime? expected_delivery_date { get; set; }

        [Comment("Ngày phê duyệt")]
        public DateTime? approved_date { get; set; }

        [Comment("Ngày hoàn thành nhập kho")]
        public DateTime? completed_date { get; set; }

        [Comment("Lý do từ chối")]
        public string? rejection_reason { get; set; }

        [Comment("Tổng tiền đơn mua hàng")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal total_amount { get; set; } = 0;

        [Comment("Ghi chú")]
        public string? note { get; set; }
    }

    [Comment("Bảng đơn mua hàng (nhập từ nhà cung cấp)")]
    [Table("purchase_orders")]
    public class purchase_order : purchase_order_properties
    {
        [ForeignKey(nameof(supplier_id))]
        public virtual supplier? supplier { get; set; }

        [ForeignKey(nameof(warehouse_id))]
        public virtual warehouse? warehouse { get; set; }

        [ForeignKey(nameof(approved_by))]
        public virtual user? approver { get; set; }

        public virtual ICollection<purchase_order_item> purchase_order_items { get; set; } = new List<purchase_order_item>();
    }

    public class purchase_order_configuration : IEntityTypeConfiguration<purchase_order>
    {
        public void Configure(EntityTypeBuilder<purchase_order> builder)
        {
            builder.HasIndex(e => e.purchase_order_code).IsUnique();
            builder.HasIndex(e => e.supplier_id);
            builder.HasIndex(e => e.warehouse_id);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.supplier)
                .WithMany(s => s.purchase_orders)
                .HasForeignKey(e => e.supplier_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.warehouse)
                .WithMany(w => w.purchase_orders)
                .HasForeignKey(e => e.warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.approver)
                .WithMany(u => u.approved_purchase_orders)
                .HasForeignKey(e => e.approved_by)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
