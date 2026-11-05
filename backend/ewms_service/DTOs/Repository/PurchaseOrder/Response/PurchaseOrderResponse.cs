using ewms_common.DTOs.Response;

namespace ewms_service.DTOs.Repository.PurchaseOrder.Response
{
    public class PurchaseOrderItemView
    {
        public string id { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string? brand { get; set; }
        public int ordered_quantity { get; set; }
        public int received_quantity { get; set; }
        public int remaining_quantity => Math.Max(0, ordered_quantity - received_quantity);
        public decimal unit_price { get; set; }
        public decimal line_total => ordered_quantity * unit_price;
    }

    public class PurchaseOrderTableView : TableViewBase
    {
        public string purchase_order_code { get; set; } = string.Empty;
        public string supplier_id { get; set; } = string.Empty;
        public string supplier_code { get; set; } = string.Empty;
        public string supplier_name { get; set; } = string.Empty;
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public DateTime order_date { get; set; }
        public DateTime? expected_delivery_date { get; set; }
        public DateTime? approved_date { get; set; }
        public DateTime? completed_date { get; set; }
        public string? approved_by { get; set; }
        public string? approver_name { get; set; }
        public int total_ordered_quantity { get; set; }
        public int total_received_quantity { get; set; }
        public decimal total_amount { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public string? rejection_reason { get; set; }
        public string? note { get; set; }
    }

    public class PurchaseOrderDetailView : PurchaseOrderTableView
    {
        public List<PurchaseOrderItemView> items { get; set; } = new();
    }
}
