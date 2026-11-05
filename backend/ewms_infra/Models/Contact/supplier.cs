using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Repository.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Contact
{
    public class supplier_properties : ExtendModel
    {
        [Comment("Mã nhà cung cấp")]
        [MaxLength(30)]
        public string supplier_code { get; set; } = string.Empty;

        [Comment("Tên nhà cung cấp")]
        [MaxLength(150)]
        public string supplier_name { get; set; } = string.Empty;

        [Comment("Người liên hệ")]
        [MaxLength(100)]
        public string? contact_person { get; set; }

        [Comment("Số điện thoại")]
        [MaxLength(20)]
        public string? phone { get; set; }

        [Comment("Địa chỉ email")]
        [MaxLength(100)]
        public string? email { get; set; }

        [Comment("Địa chỉ nhà cung cấp")]
        [MaxLength(255)]
        public string? address { get; set; }

        [Comment("Trạng thái nhà cung cấp")]
        [EnumDataType(typeof(eStatus_Supplier))]
        public eStatus_Supplier status { get; set; } = eStatus_Supplier.Active;
    }

    [Comment("Bảng danh mục nhà cung cấp")]
    [Table("suppliers")]
    public class supplier : supplier_properties
    {
        public virtual ICollection<purchase_order> purchase_orders { get; set; } = new List<purchase_order>();
    }

    public class supplier_configuration : IEntityTypeConfiguration<supplier>
    {
        public void Configure(EntityTypeBuilder<supplier> builder)
        {
            builder.HasIndex(e => e.supplier_code).IsUnique();
            builder.HasIndex(e => e.supplier_name);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
