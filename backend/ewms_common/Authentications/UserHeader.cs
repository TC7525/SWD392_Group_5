namespace ewms_common.Authentications
{
    public class UserHeader
    {
        public string user_id { get; set; } = null!;
        public string employee_code { get; set; } = null!;
        public string username { get; set; } = null!;
        public string email { get; set; } = null!;
        public string phone { get; set; } = null!;
        public string fullname { get; set; } = null!;
        public string warehouse_id { get; set; } = null!;
        public string warehouse_code { get; set; } = null!;
        public string warehouse_name { get; set; } = null!;
        public string role { get; set; } = null!;
        public string role_id { get; set; } = null!;
        public string? system_role { get; set; }
    }
}
