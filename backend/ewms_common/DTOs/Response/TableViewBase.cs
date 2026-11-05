namespace ewms_common.DTOs.Response
{
    public class TableViewBase
    {
        public string id { get; set; } = string.Empty;
        public string? created_by { get; set; }
        public DateTime? created_on { get; set; }
        public string? created_by_name { get; set; }
        public string? modified_by { get; set; }
        public DateTime? modified_on { get; set; }
        public string? modified_by_name { get; set; }
    }
}
