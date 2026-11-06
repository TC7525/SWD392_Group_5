using System;
using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinimumStockLevel { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Category? Category { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    }
}
