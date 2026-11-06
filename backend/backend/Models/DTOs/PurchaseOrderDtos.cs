using System;
using System.Collections.Generic;
using System.Linq;
using backend.Enums;

namespace backend.Models.DTOs
{
    public class CreatePurchaseOrderItemRequest
    {
        public int ProductId { get; set; }
        public int OrderedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreatePurchaseOrderRequest
    {
        public int SupplierId { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
    }

    public class PurchaseOrderItemResponse
    {
        public int PurchaseOrderItemId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        public int RemainingQuantity => OrderedQuantity - ReceivedQuantity;
        public decimal LineTotal => OrderedQuantity * UnitPrice;
    }

    public class PurchaseOrderResponse
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public POStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PurchaseOrderItemResponse> Items { get; set; } = new();

        public int TotalOrderedQuantity => Items?.Sum(i => i.OrderedQuantity) ?? 0;
        public int TotalReceivedQuantity => Items?.Sum(i => i.ReceivedQuantity) ?? 0;
        public int TotalRemainingQuantity => Items?.Sum(i => i.RemainingQuantity) ?? 0;
        public decimal GrandTotal => Items?.Sum(i => i.LineTotal) ?? 0m;
    }

    public class PurchaseOrderListResponse
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public POStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal GrandTotal
        {
            get => TotalAmount;
            set => TotalAmount = value;
        }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewPurchaseOrderRequest
    {
        public string? Reason { get; set; }
    }

    public class SupplierOptionDto
    {
        public int SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ProductOptionDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int MinimumStockLevel { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class QuickCreateProductRequest
    {
        public string? ProductCode { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
    }

    public class CategoryOptionDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class PurchaseOrderFormDataResponse
    {
        public List<SupplierOptionDto> Suppliers { get; set; } = new();
        public List<ProductOptionDto> Products { get; set; } = new();
        public List<CategoryOptionDto> Categories { get; set; } = new();
    }
}
