using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eStatus_Account
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2,
        [Description("Đã khóa")]
        Locked = 3
    }
}
