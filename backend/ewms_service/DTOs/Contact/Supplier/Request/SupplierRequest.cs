using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Contact.Supplier.Request
{
    public class SupplierBase
    {
        [TrimString(true)]
        [MaxLength(30)]
        public string? supplier_code { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống.")]
        [TrimString(true)]
        [MaxLength(150)]
        public string supplier_name { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(100)]
        public string? contact_person { get; set; }

        [TrimString(true)]
        [MaxLength(20)]
        public string? phone { get; set; }

        [TrimString(true)]
        [MaxLength(100)]
        public string? email { get; set; }

        [TrimString(true)]
        [MaxLength(255)]
        public string? address { get; set; }

        public eStatus_Supplier status { get; set; } = eStatus_Supplier.Active;
    }

    public class SupplierCreating : SupplierBase
    {
    }

    public class SupplierUpdating : SupplierBase
    {
        [Required(ErrorMessage = "Id nhà cung cấp không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class SupplierTableFilter : TableFilterBase
    {
        public eStatus_Supplier? status { get; set; }
    }
}
