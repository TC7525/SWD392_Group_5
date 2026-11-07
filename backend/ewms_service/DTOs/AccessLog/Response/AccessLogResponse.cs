using ewms_common.DTOs.Response;

namespace ewms_service.DTOs.AccessLog.Response
{
    public class AccessLogTableView : TableViewBase
    {
        public string? user_id { get; set; }
        public string? employee_code { get; set; }
        public string? full_name { get; set; }
        public string module { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string? entity_type { get; set; }
        public string? entity_id { get; set; }
        public string? description { get; set; }
        public string? ip_address { get; set; }
    }
}
