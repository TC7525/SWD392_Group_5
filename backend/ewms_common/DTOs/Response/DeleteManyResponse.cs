namespace ewms_common.DTOs.Response
{
    public class DeleteManyResponse
    {
        public int handled_number { get; set; }
        public int success_number { get; set; }
        public int failed_number { get; set; }
        public List<DeleteManyResponse_FailedItem> failed_items { get; set; } = new List<DeleteManyResponse_FailedItem>();
    }

    public class DeleteManyResponse_FailedItem
    {
        public string refid { get; set; } = null!;
        public int reftype { get; set; }
        public string? reftype_name { get; set; }
        public string? refno { get; set; }
        public DateTime? refdate { get; set; }
        public DateTime? posted_date { get; set; }
        public string error { get; set; } = null!;
    }
}
