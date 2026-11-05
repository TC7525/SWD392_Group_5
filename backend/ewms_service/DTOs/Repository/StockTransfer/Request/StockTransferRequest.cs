using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Repository.StockTransfer.Request
{
    public class StockTransferItemRequest
    {
        [Required(ErrorMessage = "Id sản phẩm không được để trống.")]
        public string product_id { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng chuyển kho phải lớn hơn 0.")]
        public int quantity { get; set; }

        [TrimString(true)]
        public string? note { get; set; }
    }

    public class StockTransferBase
    {
        [TrimString(true)]
        [MaxLength(40)]
        public string? stock_transfer_code { get; set; }

        [Required(ErrorMessage = "Kho xuất không được để trống.")]
        public string source_warehouse_id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kho nhận không được để trống.")]
        public string destination_warehouse_id { get; set; } = string.Empty;

        public DateTime? transfer_date { get; set; }

        [TrimString(true)]
        public string? note { get; set; }

        [Required(ErrorMessage = "Danh sách sản phẩm chuyển kho không được để trống.")]
        public List<StockTransferItemRequest> items { get; set; } = new();
    }

    public class StockTransferCreating : StockTransferBase
    {
    }

    public class StockTransferApproveRequest
    {
        [Required(ErrorMessage = "Id phiếu chuyển kho không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class StockTransferRejectRequest
    {
        [Required(ErrorMessage = "Id phiếu chuyển kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do từ chối không được để trống.")]
        [TrimString(true)]
        public string rejection_reason { get; set; } = string.Empty;
    }

    public class StockTransferCancelRequest
    {
        [Required(ErrorMessage = "Id phiếu chuyển kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [TrimString(true)]
        public string? cancel_reason { get; set; }
    }

    public class StockTransferDispatchRequest
    {
        [Required(ErrorMessage = "Id phiếu chuyển kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [TrimString(true)]
        public string? note { get; set; }
    }

    public class StockTransferReceiveRequest
    {
        [Required(ErrorMessage = "Id phiếu chuyển kho không được để trống.")]
        public string id { get; set; } = string.Empty;

        [TrimString(true)]
        public string? note { get; set; }
    }

    public class StockTransferTableFilter : TableFilterBase
    {
        public string? source_warehouse_id { get; set; }
        public string? destination_warehouse_id { get; set; }
        public eStatus_StockTransfer? status { get; set; }
    }
}
