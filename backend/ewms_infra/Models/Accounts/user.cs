using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.AccessLog;
using ewms_infra.Models.Repository;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_infra.Models.Repository.StockTransfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Accounts
{
    public class user_properties : ExtendModel
    {
        [Comment("Mã nhân viên")]
        [MaxLength(30)]
        public string employee_code { get; set; } = string.Empty;

        [Comment("Họ và tên")]
        [MaxLength(100)]
        public string full_name { get; set; } = string.Empty;

        [Comment("Địa chỉ email")]
        [MaxLength(100)]
        public string email { get; set; } = string.Empty;

        [Comment("Số điện thoại")]
        [MaxLength(20)]
        public string? phone { get; set; }

        [Comment("Mật khẩu đã mã hóa")]
        [MaxLength(255)]
        public string password_hash { get; set; } = string.Empty;

        [Comment("Chuỗi salt mật khẩu")]
        [MaxLength(100)]
        public string password_salt { get; set; } = string.Empty;

        [Comment("Trạng thái tài khoản")]
        [EnumDataType(typeof(eStatus_Account))]
        public eStatus_Account status { get; set; } = eStatus_Account.Active;
    }

    [Comment("Bảng người dùng hệ thống")]
    [Table("users")]
    public class user : user_properties
    {
        public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();
        public virtual ICollection<user_warehouse> user_warehouses { get; set; } = new List<user_warehouse>();
        public virtual ICollection<activity_history> activity_histories { get; set; } = new List<activity_history>();
        public virtual ICollection<purchase_order> approved_purchase_orders { get; set; } = new List<purchase_order>();
        public virtual ICollection<outbound_order> approved_outbound_orders { get; set; } = new List<outbound_order>();
        public virtual ICollection<stock_transfer> approved_stock_transfers { get; set; } = new List<stock_transfer>();
        public virtual ICollection<stock_transfer> dispatched_stock_transfers { get; set; } = new List<stock_transfer>();
        public virtual ICollection<stock_transfer> received_stock_transfers { get; set; } = new List<stock_transfer>();
        public virtual ICollection<inventory_transaction> inventory_transactions { get; set; } = new List<inventory_transaction>();
    }

    public class user_configuration : IEntityTypeConfiguration<user>
    {
        public void Configure(EntityTypeBuilder<user> builder)
        {
            builder.HasIndex(e => e.employee_code).IsUnique();
            builder.HasIndex(e => e.email).IsUnique();
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
