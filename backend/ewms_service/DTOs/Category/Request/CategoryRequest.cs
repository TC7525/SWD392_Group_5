using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Category.Request
{
    public class CategoryBase
    {
        [TrimString(true)]
        [MaxLength(30)]
        public string? category_code { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [TrimString(true)]
        [MaxLength(100)]
        public string category_name { get; set; } = string.Empty;

        [TrimString(true)]
        public string? description { get; set; }

        public eStatus_Category status { get; set; } = eStatus_Category.Active;
    }

    public class CategoryCreating : CategoryBase
    {
    }

    public class CategoryUpdating : CategoryBase
    {
        [Required(ErrorMessage = "Id danh mục không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class CategoryTableFilter : TableFilterBase
    {
        public eStatus_Category? status { get; set; }
    }
}
