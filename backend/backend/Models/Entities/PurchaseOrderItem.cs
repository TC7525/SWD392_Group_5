namespace backend.Models.Entities
{
    public class PurchaseOrderItem
    {
        public int PurchaseOrderItemId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; } = 0;
        public decimal UnitPrice { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }
        public Product? Product { get; set; }
    }
}
