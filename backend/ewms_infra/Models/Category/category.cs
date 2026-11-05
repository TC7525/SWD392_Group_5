using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ewms_common.Infras.Models;
using ewms_infra.Enums;
using ewms_infra.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ewms_infra.Models.Category
{
    public class category_properties : ExtendModel
    {
        [Comment("Mã danh mục")]
        [MaxLength(30)]
        public string category_code { get; set; } = string.Empty;

        [Comment("Tên danh mục")]
        [MaxLength(100)]
        public string category_name { get; set; } = string.Empty;

        [Comment("Mô tả danh mục")]
        public string? description { get; set; }

        [Comment("Trạng thái danh mục")]
        [EnumDataType(typeof(eStatus_Category))]
        public eStatus_Category status { get; set; } = eStatus_Category.Active;
    }

    [Comment("Bảng danh mục sản phẩm điện tử")]
    [Table("categories")]
    public class category : category_properties
    {
        public virtual ICollection<product> products { get; set; } = new List<product>();
    }

    public class category_configuration : IEntityTypeConfiguration<category>
    {
        public void Configure(EntityTypeBuilder<category> builder)
        {
            builder.HasIndex(e => e.category_code).IsUnique();
            builder.HasIndex(e => e.category_name).IsUnique();
            builder.HasIndex(e => e.status);
            builder.HasIndex(e => e.deleted);
        }
    }
}
