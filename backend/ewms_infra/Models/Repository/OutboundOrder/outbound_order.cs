using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Contact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.OutboundOrder
{
    public class outbound_order_properties : WarehouseModel
    {
        [Comment("Mã đơn xuất kho")]
        [MaxLength(40)]
        public string order_code { get; set; } = string.Empty;

        [Comment("Id khách hàng")]
        [MaxLength(36)]
        public string customer_id { get; set; } = string.Empty;

        [Comment("Id người phê duyệt")]
        [MaxLength(36)]
        public string? approved_by { get; set; }

        [Comment("Ngày tạo đơn xuất")]
        public DateTime order_date { get; set; } = DateTime.Now;

        [Comment("Ngày phê duyệt")]
        public DateTime? approved_date { get; set; }

        [Comment("Ngày hoàn thành xuất kho")]
        public DateTime? completed_date { get; set; }

        [Comment("Trạng thái đơn xuất kho")]
        [EnumDataType(typeof(eStatus_OutboundOrder))]
        public eStatus_OutboundOrder status { get; set; } = eStatus_OutboundOrder.Pending;

        [Comment("Tổng số lượng sản phẩm")]
        public int total_quantity { get; set; } = 0;

        [Comment("Tổng tiền đơn xuất")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal total_amount { get; set; } = 0;

        [Comment("Lý do từ chối")]
        public string? rejection_reason { get; set; }

        [Comment("Ghi chú")]
        public string? note { get; set; }
    }

    [Comment("Bảng đơn xuất kho bán hàng")]
    [Table("outbound_orders")]
    public class outbound_order : outbound_order_properties
    {
        [ForeignKey(nameof(customer_id))]
        public virtual customer? customer { get; set; }

        [ForeignKey(nameof(warehouse_id))]
        public virtual warehouse? warehouse { get; set; }

        [ForeignKey(nameof(approved_by))]
        public virtual user? approver { get; set; }

        public virtual ICollection<outbound_order_detail> outbound_order_details { get; set; } = new List<outbound_order_detail>();
    }

    public class outbound_order_configuration : IEntityTypeConfiguration<outbound_order>
    {
        public void Configure(EntityTypeBuilder<outbound_order> builder)
        {
            builder.HasIndex(e => e.order_code).IsUnique();
            builder.HasIndex(e => e.customer_id);
            builder.HasIndex(e => e.warehouse_id);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.customer)
                .WithMany(c => c.outbound_orders)
                .HasForeignKey(e => e.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.warehouse)
                .WithMany(w => w.outbound_orders)
                .HasForeignKey(e => e.warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.approver)
                .WithMany(u => u.approved_outbound_orders)
                .HasForeignKey(e => e.approved_by)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
