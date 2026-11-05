using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Repository.OutboundOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Contact
{
    public class customer_properties : ExtendModel
    {
        [Comment("Mã khách hàng")]
        [MaxLength(30)]
        public string customer_code { get; set; } = string.Empty;

        [Comment("Tên khách hàng")]
        [MaxLength(150)]
        public string customer_name { get; set; } = string.Empty;

        [Comment("Số điện thoại")]
        [MaxLength(20)]
        public string? phone { get; set; }

        [Comment("Địa chỉ email")]
        [MaxLength(100)]
        public string? email { get; set; }

        [Comment("Địa chỉ khách hàng")]
        [MaxLength(255)]
        public string? address { get; set; }

        [Comment("Trạng thái khách hàng")]
        [EnumDataType(typeof(eStatus_Common))]
        public eStatus_Common status { get; set; } = eStatus_Common.Active;
    }

    [Comment("Bảng danh mục khách hàng")]
    [Table("customers")]
    public class customer : customer_properties
    {
        public virtual ICollection<outbound_order> outbound_orders { get; set; } = new List<outbound_order>();
    }

    public class customer_configuration : IEntityTypeConfiguration<customer>
    {
        public void Configure(EntityTypeBuilder<customer> builder)
        {
            builder.HasIndex(e => e.customer_code).IsUnique();
            builder.HasIndex(e => e.customer_name);
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
