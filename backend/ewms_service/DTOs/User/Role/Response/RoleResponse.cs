using ewms_common.DTOs.Response;
using ewms_service.DTOs.User.Role.Request;

namespace ewms_service.DTOs.User.Role.Response
{
    public class PermissionView
    {
        public string id { get; set; } = string.Empty;
        public string permission_code { get; set; } = string.Empty;
        public string permission_name { get; set; } = string.Empty;
        public string module { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string? description { get; set; }
    }

    public class RoleTableView : TableViewBase
    {
        public string role_code { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public string? description { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public int user_count { get; set; }
        public List<PermissionView> permissions { get; set; } = new();
    }

    public class RoleDetailView : RoleBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public List<PermissionView> permissions { get; set; } = new();
    }
}
