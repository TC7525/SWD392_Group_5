using System;
using backend.Enums;

namespace backend.Models.Entities
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int UserId { get; set; }
        public int? PurchaseOrderItemId { get; set; }
        public int? OutboundOrderDetailId { get; set; }
        public int? StockTransferId { get; set; }
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        public Product? Product { get; set; }
        public Warehouse? Warehouse { get; set; }
        public User? User { get; set; }
        public PurchaseOrderItem? PurchaseOrderItem { get; set; }
    }
}
