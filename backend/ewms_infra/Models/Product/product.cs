using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Category;
using ewms_infra.Models.Repository;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_infra.Models.Repository.StockTransfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Product
{
    public class product_properties : ExtendModel
    {
        [Comment("Mã sản phẩm")]
        [MaxLength(50)]
        public string product_code { get; set; } = string.Empty;

        [Comment("Tên sản phẩm")]
        [MaxLength(150)]
        public string product_name { get; set; } = string.Empty;

        [Comment("Id danh mục")]
        [MaxLength(36)]
        public string category_id { get; set; } = string.Empty;

        [Comment("Thương hiệu")]
        [MaxLength(100)]
        public string? brand { get; set; }

        [Comment("Mô tả sản phẩm")]
        public string? description { get; set; }

        [Comment("Đơn giá niêm yết")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal unit_price { get; set; } = 0;

        [Comment("Mức tồn kho tối thiểu (cảnh báo)")]
        public int minimum_stock_level { get; set; } = 0;

        [Comment("Trạng thái sản phẩm")]
        [EnumDataType(typeof(eStatus_Product))]
        public eStatus_Product status { get; set; } = eStatus_Product.Active;
    }

    [Comment("Bảng sản phẩm thiết bị điện tử")]
    [Table("products")]
    public class product : product_properties
    {
        [ForeignKey(nameof(category_id))]
        public virtual category? category { get; set; }

        public virtual ICollection<inventory> inventories { get; set; } = new List<inventory>();
        public virtual ICollection<purchase_order_item> purchase_order_items { get; set; } = new List<purchase_order_item>();
        public virtual ICollection<outbound_order_detail> outbound_order_details { get; set; } = new List<outbound_order_detail>();
        public virtual ICollection<stock_transfer_item> stock_transfer_items { get; set; } = new List<stock_transfer_item>();
        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class product_configuration : IEntityTypeConfiguration<product>
    {
        public void Configure(EntityTypeBuilder<product> builder)
        {
            builder.HasIndex(e => e.product_code).IsUnique();
            builder.HasIndex(e => e.category_id);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);

            builder.HasOne(e => e.category)
                .WithMany(c => c.products)
                .HasForeignKey(e => e.category_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
