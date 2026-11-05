namespace ewms_service.DTOs.User.Authentication.Response
{
    public class AssignedWarehouseView
    {
        public string warehouse_id { get; set; } = string.Empty;
        public string warehouse_code { get; set; } = string.Empty;
        public string warehouse_name { get; set; } = string.Empty;
        public string? address { get; set; }
    }

    public class UserProfileView
    {
        public string id { get; set; } = string.Empty;
        public string employee_code { get; set; } = string.Empty;
        public string full_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? phone { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public string role_id { get; set; } = string.Empty;
        public string role_code { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public List<string> permissions { get; set; } = new();
        public string current_warehouse_id { get; set; } = string.Empty;
        public string current_warehouse_code { get; set; } = string.Empty;
        public string current_warehouse_name { get; set; } = string.Empty;
        public List<AssignedWarehouseView> assigned_warehouses { get; set; } = new();
    }

    public class LoginResponse
    {
        public string access_token { get; set; } = string.Empty;
        public DateTime expires_at { get; set; }
        public UserProfileView user { get; set; } = new();
    }
}
