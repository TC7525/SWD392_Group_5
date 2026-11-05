using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Contact.Customer.Request
{
    public class CustomerBase
    {
        [TrimString(true)]
        [MaxLength(30)]
        public string? customer_code { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        [TrimString(true)]
        [MaxLength(150)]
        public string customer_name { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(20)]
        public string? phone { get; set; }

        [TrimString(true)]
        [MaxLength(100)]
        public string? email { get; set; }

        [TrimString(true)]
        [MaxLength(255)]
        public string? address { get; set; }

        public eStatus_Common status { get; set; } = eStatus_Common.Active;
    }

    public class CustomerCreating : CustomerBase
    {
    }

    public class CustomerUpdating : CustomerBase
    {
        [Required(ErrorMessage = "Id khách hàng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class CustomerTableFilter : TableFilterBase
    {
        public eStatus_Common? status { get; set; }
    }
}
