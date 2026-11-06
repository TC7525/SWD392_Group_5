using System;

namespace backend.Models.Entities
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
