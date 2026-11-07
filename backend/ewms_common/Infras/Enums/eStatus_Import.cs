using System.ComponentModel;

namespace ewms_common.Infras.Enums
{
    public enum eStatus_Import
    {
        [Description("Hợp lệ")]
        Valid = 0,
        [Description("Không hợp lệ")]
        Invalid = 1
    }
}
