using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Repository.OutboundOrder.Request
{
    public class OutboundOrderItemRequest
    {
        [Required(ErrorMessage = "Id sản phẩm không được để trống.")]
        public string product_id { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng xuất phải lớn hơn 0.")]
        public int quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá bán phải lớn hơn hoặc bằng 0.")]
        public decimal? unit_price { get; set; }
    }

    public class OutboundOrderBase
    {
        [TrimString(true)]
        [MaxLength(40)]
        public string? order_code { get; set; }

        [Required(ErrorMessage = "Khách hàng không được để trống.")]
        public string customer_id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kho xuất hàng không được để trống.")]
        public string warehouse_id { get; set; } = string.Empty;

        public DateTime? order_date { get; set; }

        [TrimString(true)]
        public string? note { get; set; }

        [Required(ErrorMessage = "Danh sách sản phẩm xuất không được để trống.")]
        public List<OutboundOrderItemRequest> items { get; set; } = new();
    }

    public class OutboundOrderCreating : OutboundOrderBase
    {
    }

    public class OutboundOrderUpdating : OutboundOrderBase
    {
        [Required(ErrorMessage = "Id đơn xuất kho không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class OutboundOrderApproveRequest
    {
        [Required(ErrorMessage = "Id đơn xuất kho không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class OutboundOrderRejectRequest
    {
        [Required(ErrorMessage = "Id đơn xuất kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do từ chối không được để trống.")]
        [TrimString(true)]
        public string rejection_reason { get; set; } = string.Empty;
    }

    public class OutboundOrderCancelRequest
    {
        [Required(ErrorMessage = "Id đơn xuất kho không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class OutboundOrderIssueRequest
    {
        [Required(ErrorMessage = "Id đơn xuất kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [TrimString(true)]
        public string? note { get; set; }
    }

    public class OutboundOrderTableFilter : TableFilterBase
    {
        public string? customer_id { get; set; }
        public string? warehouse_id { get; set; }
        public eStatus_OutboundOrder? status { get; set; }
    }
}
