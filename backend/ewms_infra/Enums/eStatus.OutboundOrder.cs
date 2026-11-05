using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eStatus_OutboundOrder
    {
        [Description("Chờ duyệt")]
        Pending = 1,
        [Description("Đã duyệt")]
        Approved = 2,
        [Description("Từ chối")]
        Rejected = 3,
        [Description("Hoàn thành")]
        Completed = 4,
        [Description("Đã hủy")]
        Cancelled = 5
    }
}
