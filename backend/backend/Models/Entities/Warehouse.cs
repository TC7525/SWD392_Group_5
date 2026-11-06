using System;
using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
