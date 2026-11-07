using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Repository.PurchaseOrder.Request
{
    public class PurchaseOrderItemRequest
    {
        [Required(ErrorMessage = "Id sản phẩm không được để trống.")]
        public string product_id { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt mua phải lớn hơn 0.")]
        public int ordered_quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá mua phải lớn hơn hoặc bằng 0.")]
        public decimal unit_price { get; set; }
    }

    public class PurchaseOrderBase
    {
        [TrimString(true)]
        [MaxLength(40)]
        public string? purchase_order_code { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp không được để trống.")]
        public string supplier_id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kho nhập hàng không được để trống.")]
        public string warehouse_id { get; set; } = string.Empty;

        public DateTime? order_date { get; set; }

        public DateTime? expected_delivery_date { get; set; }

        [TrimString(true)]
        public string? note { get; set; }

        [Required(ErrorMessage = "Danh sách sản phẩm không được để trống.")]
        public List<PurchaseOrderItemRequest> items { get; set; } = new();
    }

    public class PurchaseOrderCreating : PurchaseOrderBase
    {
    }

    public class PurchaseOrderUpdating : PurchaseOrderBase
    {
        [Required(ErrorMessage = "Id đơn mua hàng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class PurchaseOrderApproveRequest
    {
        [Required(ErrorMessage = "Id đơn mua hàng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class PurchaseOrderRejectRequest
    {
        [Required(ErrorMessage = "Id đơn mua hàng không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do từ chối không được để trống.")]
        [TrimString(true)]
        public string rejection_reason { get; set; } = string.Empty;
    }

    public class PurchaseOrderCancelRequest
    {
        [Required(ErrorMessage = "Id đơn mua hàng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class PurchaseOrderReceiveItemRequest
    {
        [Required(ErrorMessage = "Id dòng sản phẩm không được để trống.")]
        public string purchase_order_item_id { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng thực nhập phải lớn hơn 0.")]
        public int receive_quantity { get; set; }
    }

    public class PurchaseOrderReceiveRequest
    {
        [Required(ErrorMessage = "Id đơn mua hàng không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh sách sản phẩm thực nhập không được để trống.")]
        public List<PurchaseOrderReceiveItemRequest> items { get; set; } = new();

        [TrimString(true)]
        public string? note { get; set; }
    }

    public class PurchaseOrderTableFilter : TableFilterBase
    {
        public string? supplier_id { get; set; }
        public string? warehouse_id { get; set; }
        public eStatus_PurchaseOrder? status { get; set; }
    }
}
