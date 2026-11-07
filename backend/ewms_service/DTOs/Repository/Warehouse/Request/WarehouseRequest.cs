using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Repository.Warehouse.Request
{
    public class WarehouseBase
    {
        [TrimString(true)]
        [MaxLength(30)]
        public string? warehouse_code { get; set; }

        [Required(ErrorMessage = "Tên kho không được để trống.")]
        [TrimString(true)]
        [MaxLength(150)]
        public string warehouse_name { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(255)]
        public string? address { get; set; }

        public eStatus_Warehouse status { get; set; } = eStatus_Warehouse.Active;
    }

    public class WarehouseCreating : WarehouseBase
    {
    }

    public class WarehouseUpdating : WarehouseBase
    {
        [Required(ErrorMessage = "Id kho hàng không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class WarehouseTableFilter : TableFilterBase
    {
        public eStatus_Warehouse? status { get; set; }
    }
}
