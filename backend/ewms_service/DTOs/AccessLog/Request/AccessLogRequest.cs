using ewms_common.DTOs.Request;

namespace ewms_service.DTOs.AccessLog.Request
{
    public class AccessLogCreating
    {
        public string? user_id { get; set; }
        public string module { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string? entity_type { get; set; }
        public string? entity_id { get; set; }
        public string? description { get; set; }
        public string? ip_address { get; set; }
    }

    public class AccessLogTableFilter : TableFilterBase
    {
        public string? user_id { get; set; }
        public string? module { get; set; }
        public string? action { get; set; }
    }
}
