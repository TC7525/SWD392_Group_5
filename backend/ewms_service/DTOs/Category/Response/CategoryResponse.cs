using ewms_common.DTOs.Response;
using ewms_service.DTOs.Category.Request;

namespace ewms_service.DTOs.Category.Response
{
    public class CategoryTableView : TableViewBase
    {
        public string category_code { get; set; } = string.Empty;
        public string category_name { get; set; } = string.Empty;
        public string? description { get; set; }
        public int status { get; set; }
        public string status_name { get; set; } = string.Empty;
        public int product_count { get; set; }
    }

    public class CategoryDetailView : CategoryBase
    {
        public string id { get; set; } = string.Empty;
        public string status_name { get; set; } = string.Empty;
        public int product_count { get; set; }
        public DateTime? created_on { get; set; }
        public DateTime? modified_on { get; set; }
    }
}
