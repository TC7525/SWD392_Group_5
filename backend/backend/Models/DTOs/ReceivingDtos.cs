using System.Collections.Generic;

namespace backend.Models.DTOs
{
    public class CheckQuantityItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckQuantityItemResult
    {
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int RequestedQuantity { get; set; }
        public int Quantity
        {
            get => RequestedQuantity;
            set => RequestedQuantity = value;
        }
        public int RemainingQuantity { get; set; }
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }

    public class CheckQuantityResponse
    {
        public bool Valid { get; set; }
        public string? Message { get; set; }
        public List<CheckQuantityItemResult> Items { get; set; } = new();
    }

    public class ConfirmReceiveItemRequest
    {
        public int PurchaseOrderItemId { get; set; }
        public int ReceivedQuantity { get; set; }
        public int Quantity
        {
            get => ReceivedQuantity;
            set => ReceivedQuantity = value;
        }
        public int? ProductId { get; set; }
    }

    public class ConfirmReceiveRequest
    {
        public int WarehouseId { get; set; }
        public string? Note { get; set; }
        public List<ConfirmReceiveItemRequest> Items { get; set; } = new();
    }
}
