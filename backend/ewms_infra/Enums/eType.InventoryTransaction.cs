using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eType_InventoryTransaction
    {
        [Description("Nhập kho")]
        Inbound = 1,
        [Description("Xuất kho")]
        Outbound = 2,
        [Description("Xuất chuyển kho")]
        TransferOut = 3,
        [Description("Nhập chuyển kho")]
        TransferIn = 4,
        [Description("Điều chỉnh tồn kho")]
        Adjustment = 5
    }
}
