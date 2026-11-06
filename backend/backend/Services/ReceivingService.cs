using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Common;
using backend.Enums;
using backend.Models;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Repositories;

namespace backend.Services
{
    public class ReceivingService : IReceivingService
    {
        private readonly AppDbContext _context;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryTransactionRepository _transactionRepository;
        private readonly IPOStatusService _poStatusService;

        public ReceivingService(
            AppDbContext context,
            IPurchaseOrderRepository purchaseOrderRepository,
            IInventoryRepository inventoryRepository,
            IInventoryTransactionRepository transactionRepository,
            IPOStatusService poStatusService)
        {
            _context = context;
            _purchaseOrderRepository = purchaseOrderRepository;
            _inventoryRepository = inventoryRepository;
            _transactionRepository = transactionRepository;
            _poStatusService = poStatusService;
        }

        public async Task<List<PurchaseOrderListResponse>> GetReceivableOrdersAsync()
        {
            var allOrders = await _purchaseOrderRepository.GetAllAsync();
            var receivable = allOrders
                .Where(o => o.Status == POStatus.Approved || o.Status == POStatus.PartiallyReceived)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.PurchaseOrderId)
                .ToList();

            return receivable.Select(MapToListResponse).ToList();
        }

        public async Task<CheckQuantityResponse> CheckQuantityAsync(int poId, List<CheckQuantityItemRequest> items)
        {
            if (items == null || items.Count == 0)
            {
                throw new AppException("MSG01", "Required information is missing.", 400);
            }

            var po = await _purchaseOrderRepository.GetByIdAsync(poId);
            if (po == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            var results = new List<CheckQuantityItemResult>();

            foreach (var itemReq in items)
            {
                var poItem = po.Items?.FirstOrDefault(i => i.ProductId == itemReq.ProductId);
                var itemResult = new CheckQuantityItemResult
                {
                    ProductId = itemReq.ProductId,
                    RequestedQuantity = itemReq.Quantity,
                    Quantity = itemReq.Quantity
                };

                if (poItem == null)
                {
                    itemResult.IsValid = false;
                    itemResult.RemainingQuantity = 0;
                    itemResult.Message = "MSG05: The selected product was not found on this purchase order.";
                }
                else
                {
                    itemResult.ProductCode = poItem.Product?.ProductCode;
                    itemResult.ProductName = poItem.Product?.ProductName;
                    int remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;
                    itemResult.RemainingQuantity = remaining;

                    if (itemReq.Quantity <= 0)
                    {
                        itemResult.IsValid = false;
                        itemResult.Message = "MSG02: Quantity must be greater than 0.";
                    }
                    else if (itemReq.Quantity > remaining)
                    {
                        itemResult.IsValid = false;
                        itemResult.Message = "MSG09: Received quantity cannot exceed the remaining ordered quantity.";
                    }
                    else
                    {
                        itemResult.IsValid = true;
                        itemResult.Message = null;
                    }
                }

                results.Add(itemResult);
            }

            bool allValid = results.All(r => r.IsValid);
            var response = new CheckQuantityResponse
            {
                Valid = allValid,
                Items = results,
                Message = allValid
                    ? "All quantities are valid."
                    : results.FirstOrDefault(r => !r.IsValid)?.Message
            };

            return response;
        }

        public async Task<PurchaseOrderResponse> StartReceivingAsync(int poId, int staffId)
        {
            var po = await _purchaseOrderRepository.GetByIdAsync(poId);
            if (po == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            _poStatusService.ValidateTransition(po.Status, POStatus.Processing);

            po.Status = POStatus.Processing;
            po.UpdatedAt = DateTime.UtcNow;

            await _purchaseOrderRepository.UpdateAsync(po);

            var updatedPo = await _purchaseOrderRepository.GetByIdAsync(po.PurchaseOrderId) ?? po;
            return MapToResponse(updatedPo);
        }

        public async Task<PurchaseOrderResponse> CancelReceivingAsync(int poId, int staffId)
        {
            var po = await _purchaseOrderRepository.GetByIdAsync(poId);
            if (po == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            if (po.Status != POStatus.Processing)
            {
                throw new AppException("MSG04", "This order cannot be processed in its current status.", 400);
            }

            bool anyReceived = po.Items != null && po.Items.Any(i => i.ReceivedQuantity > 0);
            var targetStatus = anyReceived ? POStatus.PartiallyReceived : POStatus.Approved;

            _poStatusService.ValidateTransition(po.Status, targetStatus);

            po.Status = targetStatus;
            po.UpdatedAt = DateTime.UtcNow;

            await _purchaseOrderRepository.UpdateAsync(po);

            var updatedPo = await _purchaseOrderRepository.GetByIdAsync(po.PurchaseOrderId) ?? po;
            return MapToResponse(updatedPo);
        }

        public async Task<PurchaseOrderResponse> ConfirmReceiveAsync(int poId, ConfirmReceiveRequest request, int staffId)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                throw new AppException("MSG01", "Required information is missing.", 400);
            }

            var po = await _purchaseOrderRepository.GetByIdAsync(poId);
            if (po == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            if (po.Status != POStatus.Processing)
            {
                throw new AppException("MSG04", "This order cannot be processed in its current status.", 400);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == request.WarehouseId);
                if (warehouse == null || !string.Equals(warehouse.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AppException("MSG17", "The operation could not be completed. No inventory changes were saved.", 400);
                }

                foreach (var itemReq in request.Items)
                {
                    PurchaseOrderItem? poItem = null;
                    if (itemReq.PurchaseOrderItemId > 0)
                    {
                        poItem = po.Items?.FirstOrDefault(i => i.PurchaseOrderItemId == itemReq.PurchaseOrderItemId);
                    }
                    else if (itemReq.ProductId.HasValue && itemReq.ProductId.Value > 0)
                    {
                        poItem = po.Items?.FirstOrDefault(i => i.ProductId == itemReq.ProductId.Value);
                    }

                    if (poItem == null)
                    {
                        throw new AppException("MSG05", "The selected product was not found.", 400);
                    }

                    int receivedQty = itemReq.ReceivedQuantity;
                    int remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;

                    if (receivedQty <= 0 || receivedQty > remaining)
                    {
                        throw new AppException("MSG09", "Received quantity cannot exceed the remaining ordered quantity.", 400);
                    }

                    poItem.ReceivedQuantity += receivedQty;
                    _context.PurchaseOrderItems.Update(poItem);

                    var inventory = _context.Inventories.Local
                        .FirstOrDefault(inv => inv.ProductId == poItem.ProductId && inv.WarehouseId == request.WarehouseId)
                        ?? await _context.Inventories
                            .FirstOrDefaultAsync(inv => inv.ProductId == poItem.ProductId && inv.WarehouseId == request.WarehouseId);

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = poItem.ProductId,
                            WarehouseId = request.WarehouseId,
                            Quantity = receivedQty,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        await _context.Inventories.AddAsync(inventory);
                    }
                    else
                    {
                        inventory.Quantity += receivedQty;
                        inventory.LastUpdatedAt = DateTime.UtcNow;
                        _context.Inventories.Update(inventory);
                    }

                    var inventoryTx = new InventoryTransaction
                    {
                        ProductId = poItem.ProductId,
                        WarehouseId = request.WarehouseId,
                        UserId = staffId,
                        PurchaseOrderItemId = poItem.PurchaseOrderItemId,
                        TransactionType = TransactionType.IN,
                        Quantity = receivedQty,
                        TransactionDate = DateTime.UtcNow,
                        Note = request.Note
                    };
                    await _context.InventoryTransactions.AddAsync(inventoryTx);
                }

                bool allCompleted = po.Items != null && po.Items.All(i => i.ReceivedQuantity == i.OrderedQuantity);
                if (allCompleted)
                {
                    _poStatusService.ValidateTransition(po.Status, POStatus.Completed);
                    po.Status = POStatus.Completed;
                    po.CompletedDate = DateTime.UtcNow;
                }
                else
                {
                    _poStatusService.ValidateTransition(po.Status, POStatus.PartiallyReceived);
                    po.Status = POStatus.PartiallyReceived;
                }

                po.UpdatedAt = DateTime.UtcNow;
                _context.PurchaseOrders.Update(po);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (AppException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw new AppException("MSG17", "The operation could not be completed. No inventory changes were saved.", ex, 500);
            }

            var updatedPo = await _purchaseOrderRepository.GetByIdAsync(po.PurchaseOrderId) ?? po;
            return MapToResponse(updatedPo);
        }

        private static PurchaseOrderResponse MapToResponse(PurchaseOrder po)
        {
            return new PurchaseOrderResponse
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PurchaseOrderCode = po.PurchaseOrderCode,
                SupplierId = po.SupplierId,
                SupplierCode = po.Supplier?.SupplierCode,
                SupplierName = po.Supplier?.SupplierName,
                CreatedBy = po.CreatedBy,
                CreatedByName = po.Creator?.FullName,
                ApprovedBy = po.ApprovedBy,
                ApprovedByName = po.Approver?.FullName,
                Status = po.Status,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                ApprovedDate = po.ApprovedDate,
                CompletedDate = po.CompletedDate,
                RejectionReason = po.RejectionReason,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt,
                Items = po.Items?.Select(i => new PurchaseOrderItemResponse
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    PurchaseOrderId = i.PurchaseOrderId,
                    ProductId = i.ProductId,
                    ProductCode = i.Product?.ProductCode,
                    ProductName = i.Product?.ProductName,
                    OrderedQuantity = i.OrderedQuantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    UnitPrice = i.UnitPrice
                }).ToList() ?? new List<PurchaseOrderItemResponse>()
            };
        }

        private static PurchaseOrderListResponse MapToListResponse(PurchaseOrder po)
        {
            var items = po.Items?.ToList() ?? new List<PurchaseOrderItem>();
            return new PurchaseOrderListResponse
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PurchaseOrderCode = po.PurchaseOrderCode,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier?.SupplierName ?? string.Empty,
                CreatedBy = po.CreatedBy,
                CreatedByName = po.Creator?.FullName ?? string.Empty,
                ApprovedBy = po.ApprovedBy,
                ApprovedByName = po.Approver?.FullName,
                Status = po.Status,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                TotalItems = items.Count,
                TotalQuantity = items.Sum(i => i.OrderedQuantity),
                TotalAmount = items.Sum(i => i.OrderedQuantity * i.UnitPrice),
                CreatedAt = po.CreatedAt
            };
        }
    }
}
