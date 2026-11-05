using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eType_AccessLogAction
    {
        [Description("Đăng nhập")]
        Login = 1,
        [Description("Đăng xuất")]
        Logout = 2,
        [Description("Tạo mới")]
        Create = 3,
        [Description("Cập nhật")]
        Update = 4,
        [Description("Xóa")]
        Delete = 5,
        [Description("Phê duyệt")]
        Approve = 6,
        [Description("Từ chối")]
        Reject = 7,
        [Description("Hủy")]
        Cancel = 8,
        [Description("Nhập kho")]
        ReceiveGoods = 9,
        [Description("Xuất kho")]
        IssueGoods = 10,
        [Description("Xuất chuyển kho")]
        DispatchTransfer = 11,
        [Description("Nhận chuyển kho")]
        ReceiveTransfer = 12,
        [Description("Đổi mật khẩu")]
        ChangePassword = 13,
        [Description("Đặt lại mật khẩu")]
        ResetPassword = 14
    }
}
