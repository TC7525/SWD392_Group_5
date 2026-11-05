using ewms_common.Attributes;
using ewms_common.Wrappers;

namespace ewms_common.DTOs.Request
{
    public class TableFilterBase : Paging
    {
        public string? keyword { get; set; }
        public DateTime? from_date { get; set; }
        [DateGreaterThan(nameof(from_date))]
        public DateTime? to_date { get; set; }
    }
}
