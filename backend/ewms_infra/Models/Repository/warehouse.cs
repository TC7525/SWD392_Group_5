using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_infra.Models.Repository.StockTransfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Repository
{
    public class warehouse_properties : ExtendModel
    {
        [Comment("Mã kho")]
        [MaxLength(30)]
        public string warehouse_code { get; set; } = string.Empty;

        [Comment("Tên kho")]
        [MaxLength(150)]
        public string warehouse_name { get; set; } = string.Empty;

        [Comment("Địa chỉ kho")]
        [MaxLength(255)]
        public string? address { get; set; }

        [Comment("Trạng thái kho")]
        [EnumDataType(typeof(eStatus_Warehouse))]
        public eStatus_Warehouse status { get; set; } = eStatus_Warehouse.Active;
    }

    [Comment("Bảng danh mục kho hàng")]
    [Table("warehouses")]
    public class warehouse : warehouse_properties
    {
        public virtual ICollection<user_warehouse> user_warehouses { get; set; } = new List<user_warehouse>();
        public virtual ICollection<inventory> inventories { get; set; } = new List<inventory>();
        public virtual ICollection<purchase_order> purchase_orders { get; set; } = new List<purchase_order>();
        public virtual ICollection<outbound_order> outbound_orders { get; set; } = new List<outbound_order>();
        public virtual ICollection<stock_transfer> source_stock_transfers { get; set; } = new List<stock_transfer>();
        public virtual ICollection<stock_transfer> destination_stock_transfers { get; set; } = new List<stock_transfer>();
        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class warehouse_configuration : IEntityTypeConfiguration<warehouse>
    {
        public void Configure(EntityTypeBuilder<warehouse> builder)
        {
            builder.HasIndex(e => e.warehouse_code).IsUnique();
            builder.HasIndex(e => e.warehouse_name).IsUnique();
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
