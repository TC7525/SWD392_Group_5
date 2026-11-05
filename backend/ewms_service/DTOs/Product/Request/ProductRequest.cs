using System.ComponentModel.DataAnnotations;
using ewms_common.Attributes;
using ewms_common.DTOs.Request;
using ewms_infra.Enums;

namespace ewms_service.DTOs.Product.Request
{
    public class ProductBase
    {
        [TrimString(true)]
        [MaxLength(50)]
        public string? product_code { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [TrimString(true)]
        [MaxLength(150)]
        public string product_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục sản phẩm không được để trống.")]
        public string category_id { get; set; } = string.Empty;

        [TrimString(true)]
        [MaxLength(100)]
        public string? brand { get; set; }

        [TrimString(true)]
        public string? description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0.")]
        public decimal unit_price { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối thiểu phải lớn hơn hoặc bằng 0.")]
        public int minimum_stock_level { get; set; } = 0;

        public eStatus_Product status { get; set; } = eStatus_Product.Active;
    }

    public class ProductCreating : ProductBase
    {
    }

    public class ProductUpdating : ProductBase
    {
        [Required(ErrorMessage = "Id sản phẩm không được để trống.")]
        public string id { get; set; } = string.Empty;
    }

    public class ProductTableFilter : TableFilterBase
    {
        public string? category_id { get; set; }
        public string? warehouse_id { get; set; }
        public bool? low_stock_only { get; set; }
        public eStatus_Product? status { get; set; }
    }
}
