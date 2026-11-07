using ewms_common.Infras.Enums;

namespace ewms_common.Wrappers
{
    public class Paging
    {
        public Paging()
        {
        }
        public List<FilterTable> filters { get; set; } = new List<FilterTable>();
        public SortTable? sort { get; set; }
        public virtual int page_size { get; set; } = 20;
        public int page_index { get; set; } = 1;
    }

    public class FilterTable
    {
        public string name { get; set; } = null!;
        public string? value { get; set; }
        public eType_FilterTableColumn type { get; set; }
    }

    public class SortTable
    {
        public string name { get; set; } = null!;
        public eType_SortTableColumn type { get; set; }
    }

    public class Paged : Paging
    {
        public int total_items { get; set; }
    }
}
