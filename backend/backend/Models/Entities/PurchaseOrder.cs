using System;
using System.Collections.Generic;
using backend.Enums;

namespace backend.Models.Entities
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public POStatus Status { get; set; } = POStatus.Pending;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Supplier? Supplier { get; set; }
        public User? Creator { get; set; }
        public User? Approver { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
