using ewms_common.DTOs.Response;

namespace ewms_service.DTOs.Repository.StockTransfer.Response
{
    public class StockTransferItemView
    {
        public string id { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string? brand { get; set; }
        public int quantity { get; set; }
        public int shipped_quantity { get; set; }
        public int received_quantity { get; set; }
        public int source_warehouse_stock { get; set; }
        public string? note { get; set; }
    }

    public class StockTransferTableView : TableViewBase
    {
        public string stock_transfer_code { get; set; } = string.Empty;
        public string source_warehouse_id { get; set; } = string.Empty;
        public string source_warehouse_code { get; set; } = string.Empty;
        public string source_warehouse_name { get; set; } = string.Empty;
        public string destination_warehouse_id { get; set; } = string.Empty;
        public string destination_warehouse_code { get; set; } = string.Empty;
        public string destination_warehouse_name { get; set; } = string.Empty;
        public DateTime transfer_date { get; set; }
        public DateTime? approved_date { get; set; }
        public DateTime? dispatched_date { get; set; }
        public DateTime? completed_date { get; set; }
        public string? approved_by { get; set; }
        public string? approver_name { get; set; }
        public string? dispatched_by { get; set; }
        public string? dispatcher_name { get; set; }
        public string? received_by { get; set; }
        public string? receiver_name { get; set; }
        public int total_quantity { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public string? rejection_reason { get; set; }
        public string? cancel_reason { get; set; }
        public string? note { get; set; }
    }

    public class StockTransferDetailView : StockTransferTableView
    {
        public List<StockTransferItemView> items { get; set; } = new();
    }
}
