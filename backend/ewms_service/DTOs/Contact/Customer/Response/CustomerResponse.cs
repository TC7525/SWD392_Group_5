using ewms_common.DTOs.Response;
using ewms_service.DTOs.Contact.Customer.Request;

namespace ewms_service.DTOs.Contact.Customer.Response
{
    public class CustomerTableView : TableViewBase
    {
        public string customer_code { get; set; } = string.Empty;
        public string customer_name { get; set; } = string.Empty;
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public int total_orders { get; set; }
    }

    public class CustomerDetailView : CustomerBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public int total_orders { get; set; }
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
