using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Repository.InventoryTransaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository.StockTransfer
{
    public class stock_transfer_properties : ExtendModel
    {
        [Comment("Mã phiếu chuyển kho")]
        [MaxLength(40)]
        public string stock_transfer_code { get; set; } = string.Empty;

        [Comment("Id kho xuất (nguồn)")]
        [MaxLength(36)]
        public string source_warehouse_id { get; set; } = string.Empty;

        [Comment("Id kho nhận (đích)")]
        [MaxLength(36)]
        public string destination_warehouse_id { get; set; } = string.Empty;

        [Comment("Id người phê duyệt")]
        [MaxLength(36)]
        public string? approved_by { get; set; }

        [Comment("Id nhân viên xuất chuyển kho")]
        [MaxLength(36)]
        public string? dispatched_by { get; set; }

        [Comment("Id nhân viên nhận chuyển kho")]
        [MaxLength(36)]
        public string? received_by { get; set; }

        [Comment("Trạng thái phiếu chuyển kho")]
        [EnumDataType(typeof(eStatus_StockTransfer))]
        public eStatus_StockTransfer status { get; set; } = eStatus_StockTransfer.Pending;

        [Comment("Ngày tạo yêu cầu chuyển kho")]
        public DateTime transfer_date { get; set; } = DateTime.Now;

        [Comment("Ngày phê duyệt")]
        public DateTime? approved_date { get; set; }

        [Comment("Ngày xuất kho nguồn")]
        public DateTime? dispatched_date { get; set; }

        [Comment("Ngày hoàn thành nhận kho đích")]
        public DateTime? completed_date { get; set; }

        [Comment("Lý do từ chối")]
        public string? rejection_reason { get; set; }

        [Comment("Lý do hủy")]
        public string? cancel_reason { get; set; }

        [Comment("Ghi chú")]
        public string? note { get; set; }
    }

    [Comment("Bảng phiếu chuyển kho nội bộ")]
    [Table("stock_transfers")]
    public class stock_transfer : stock_transfer_properties
    {
        [ForeignKey(nameof(source_warehouse_id))]
        public virtual warehouse? source_warehouse { get; set; }

        [ForeignKey(nameof(destination_warehouse_id))]
        public virtual warehouse? destination_warehouse { get; set; }

        [ForeignKey(nameof(approved_by))]
        public virtual user? approver { get; set; }

        [ForeignKey(nameof(dispatched_by))]
        public virtual user? dispatcher { get; set; }

        [ForeignKey(nameof(received_by))]
        public virtual user? receiver { get; set; }

        public virtual ICollection<stock_transfer_item> stock_transfer_items { get; set; } = new List<stock_transfer_item>();
        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class stock_transfer_configuration : IEntityTypeConfiguration<stock_transfer>
    {
        public void Configure(EntityTypeBuilder<stock_transfer> builder)
        {
            builder.HasIndex(e => e.stock_transfer_code).IsUnique();
            builder.HasIndex(e => e.source_warehouse_id);
            builder.HasIndex(e => e.destination_warehouse_id);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.source_warehouse)
                .WithMany(w => w.source_stock_transfers)
                .HasForeignKey(e => e.source_warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.destination_warehouse)
                .WithMany(w => w.destination_stock_transfers)
                .HasForeignKey(e => e.destination_warehouse_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.approver)
                .WithMany(u => u.approved_stock_transfers)
                .HasForeignKey(e => e.approved_by)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.dispatcher)
                .WithMany(u => u.dispatched_stock_transfers)
                .HasForeignKey(e => e.dispatched_by)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.receiver)
                .WithMany(u => u.received_stock_transfers)
                .HasForeignKey(e => e.received_by)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
