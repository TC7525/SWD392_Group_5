using ewms_common.DTOs.Response;

namespace ewms_service.DTOs.Repository.OutboundOrder.Response
{
    public class OutboundOrderItemView
    {
        public string id { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string? brand { get; set; }
        public int quantity { get; set; }
        public int issued_quantity { get; set; }
        public int current_stock_in_warehouse { get; set; }
        public decimal unit_price { get; set; }
        public decimal line_total => quantity * unit_price;
    }

    public class OutboundOrderTableView : TableViewBase
    {
        public string order_code { get; set; } = string.Empty;
        public string customer_id { get; set; } = string.Empty;
        public string customer_code { get; set; } = string.Empty;
        public string customer_name { get; set; } = string.Empty;
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public DateTime order_date { get; set; }
        public DateTime? approved_date { get; set; }
        public DateTime? completed_date { get; set; }
        public string? approved_by { get; set; }
        public string? approver_name { get; set; }
        public int total_quantity { get; set; }
        public decimal total_amount { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public string? rejection_reason { get; set; }
        public string? note { get; set; }
    }

    public class OutboundOrderDetailView : OutboundOrderTableView
    {
        public List<OutboundOrderItemView> items { get; set; } = new();
    }
}
