using ewms_common.DTOs.Response;
using ewms_service.DTOs.Contact.Supplier.Request;

namespace ewms_service.DTOs.Contact.Supplier.Response
{
    public class SupplierTableView : TableViewBase
    {
        public string supplier_code { get; set; } = string.Empty;
        public string supplier_name { get; set; } = string.Empty;
        public string? contact_person { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public int total_purchase_orders { get; set; }
    }

    public class SupplierDetailView : SupplierBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public int total_purchase_orders { get; set; }
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
