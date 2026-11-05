using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eStatus_StockTransfer
    {
        [Description("Chờ duyệt")]
        Pending = 1,
        [Description("Đã duyệt")]
        Approved = 2,
        [Description("Từ chối")]
        Rejected = 3,
        [Description("Đang vận chuyển")]
        InTransit = 4,
        [Description("Hoàn thành")]
        Completed = 5,
        [Description("Đã hủy")]
        Cancelled = 6
    }
}
