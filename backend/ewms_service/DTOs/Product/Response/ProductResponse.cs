using ewms_common.DTOs.Response;
using ewms_service.DTOs.Product.Request;

namespace ewms_service.DTOs.Product.Response
{
    public class ProductWarehouseStockView
    {
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public int quantity { get; set; }
        public bool is_low_stock { get; set; }
        public DateTime? last_updated_at { get; set; }
    }

    public class ProductTableView : TableViewBase
    {
        public string product_code { get; set; } = string.Empty;
        public string product_name { get; set; } = string.Empty;
        public string category_id { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public string? brand { get; set; }
        public string? description { get; set; }
        public decimal unit_price { get; set; }
        public int minimum_stock_level { get; set; }
        public int current_warehouse_stock { get; set; }
        public int total_stock { get; set; }
        public bool is_low_stock { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
    }

    public class ProductDetailView : ProductBase
    {
        public string id { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public int total_stock { get; set; }
        public List<ProductWarehouseStockView> warehouse_stocks { get; set; } = new();
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
