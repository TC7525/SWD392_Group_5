using ewms_common.DTOs.Response;
using ewms_service.DTOs.User.Account.Request;
using ewms_service.DTOs.User.Authentication.Response;

namespace ewms_service.DTOs.User.Account.Response
{
    public class AccountTableView : TableViewBase
    {
        public string employee_code { get; set; } = string.Empty;
        public string full_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? phone { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public string role_id { get; set; } = string.Empty;
        public string role_code { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public List<AssignedWarehouseView> assigned_warehouses { get; set; } = new();
    }

    public class AccountDetailView : AccountBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public string role_code { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public List<AssignedWarehouseView> assigned_warehouses { get; set; } = new();
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
