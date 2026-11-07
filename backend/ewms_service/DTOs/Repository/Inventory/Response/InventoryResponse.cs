using ewms_common.DTOs.Response;

namespace ewms_service.DTOs.Repository.Inventory.Response
{
    public class InventoryTableView : TableViewBase
    {
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string category_id { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public string? brand { get; set; }
        public decimal unit_price { get; set; }
        public int quantity { get; set; }
        public int minimum_stock_level { get; set; }
        public bool is_low_stock { get; set; }
        public decimal total_stock_value => quantity * unit_price;
        public DateTime last_updated_at { get; set; }
    }

    public class InventoryTransactionTableView : TableViewBase
    {
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
        public string employee_code { get; set; } = string.Empty;
        public string user_full_name { get; set; } = string.Empty;
        public int transaction_type { get; set; }
        public string transaction_type_name { get; set; } = string.Empty;
        public int quantity { get; set; }
        public DateTime transaction_date { get; set; }
        public string? reference_code { get; set; }
        public string? note { get; set; }
    }

    public class DashboardSummaryView
    {
        public int total_products { get; set; }
        public int total_warehouses { get; set; }
        public int total_suppliers { get; set; }
        public int total_customers { get; set; }
        public int total_stock_units { get; set; }
        public decimal total_inventory_value { get; set; }
        public int low_stock_alert_count { get; set; }
        public int pending_purchase_orders { get; set; }
        public int pending_outbound_orders { get; set; }
        public int pending_stock_transfers { get; set; }
        public List<InventoryTableView> low_stock_items { get; set; } = new();
        public List<InventoryTransactionTableView> recent_transactions { get; set; } = new();
    }
}
