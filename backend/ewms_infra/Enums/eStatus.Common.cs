using System.ComponentModel;

namespace ewms_infra.Enums
{
    public enum eStatus_Common
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2
    }

    public enum eStatus_Warehouse
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2
    }

    public enum eStatus_Supplier
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2
    }

    public enum eStatus_Category
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2
    }

    public enum eStatus_Product
    {
        [Description("Hoạt động")]
        Active = 1,
        [Description("Ngừng hoạt động")]
        Inactive = 2,
        [Description("Ngừng kinh doanh")]
        Discontinued = 3
    }
}
