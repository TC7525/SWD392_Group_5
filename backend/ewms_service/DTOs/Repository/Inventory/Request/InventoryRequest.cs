using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Repository.Inventory.Request
{
    public class InventoryTableFilter : TableFilterBase
    {
        public string? warehouse_id { get; set; }
        public string? category_id { get; set; }
        public bool? low_stock_only { get; set; }
    }

    public class InventoryAdjustmentRequest
    {
        [Required(ErrorMessage = "Id kho không được để trống.")]
        public string warehouse_id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Id sản phẩm không được để trống.")]
        public string product_id { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho thực tế phải lớn hơn hoặc bằng 0.")]
        public int new_quantity { get; set; }

        [Required(ErrorMessage = "Lý do điều chỉnh tồn kho không được để trống.")]
        [TrimString(true)]
        public string note { get; set; } = string.Empty;
    }

    public class InventoryTransactionTableFilter : TableFilterBase
    {
        public string? warehouse_id { get; set; }
        public string? product_id { get; set; }
        public eType_InventoryTransaction? transaction_type { get; set; }
    }
}
