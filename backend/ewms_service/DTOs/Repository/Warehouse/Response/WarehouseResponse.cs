using ewms_common.DTOs.Response;
using ewms_service.DTOs.Repository.Warehouse.Request;

namespace ewms_service.DTOs.Repository.Warehouse.Response
{
    public class WarehouseTableView : TableViewBase
    {
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public string? address { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public int total_products { get; set; }
        public int total_quantity { get; set; }
        public int assigned_staff_count { get; set; }
    }

    public class WarehouseDetailView : WarehouseBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public int total_products { get; set; }
        public int total_quantity { get; set; }
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
