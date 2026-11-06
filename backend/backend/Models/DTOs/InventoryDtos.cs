using System;
using backend.Enums;

namespace backend.Models.DTOs
{
    public class InventoryStockResponse
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class InventoryTransactionResponse
    {
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? PurchaseOrderItemId { get; set; }
        public string? PurchaseOrderCode { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionTypeName => TransactionType.ToString();
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Note { get; set; }
    }
}
