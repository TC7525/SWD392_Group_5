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
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPOStatusService _poStatusService;
        private readonly AppDbContext? _context;

        public PurchaseOrderService(
            IPurchaseOrderRepository purchaseOrderRepository,
            ISupplierRepository supplierRepository,
            IProductRepository productRepository,
            IPOStatusService poStatusService,
            AppDbContext? context = null)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _supplierRepository = supplierRepository;
            _productRepository = productRepository;
            _poStatusService = poStatusService;
            _context = context;
        }

        public async Task<PurchaseOrderResponse> CreateOrderAsync(CreatePurchaseOrderRequest request, int userId)
        {
            if (request == null)
            {
                throw new AppException("MSG01", "Required information is missing.", 400);
            }

            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
            if (supplier == null || !string.Equals(supplier.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException("MSG15", "The selected supplier is inactive and cannot be used.", 400);
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new AppException("MSG01", "Required information is missing.", 400);
            }

            // Group duplicate items by ProductId if any
            var groupedItems = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new CreatePurchaseOrderItemRequest
                {
                    ProductId = g.Key,
                    OrderedQuantity = g.Sum(x => x.OrderedQuantity),
                    UnitPrice = g.First().UnitPrice
                })
                .ToList();

            var poItems = new List<PurchaseOrderItem>();
            foreach (var itemReq in groupedItems)
            {
                if (itemReq.OrderedQuantity <= 0)
                {
                    throw new AppException("MSG02", "Quantity must be greater than 0.", 400);
                }

                if (itemReq.UnitPrice < 0)
                {
                    throw new AppException("MSG01", "Required information is missing.", 400);
                }

                var product = await _productRepository.GetByIdAsync(itemReq.ProductId);
                if (product == null)
                {
                    throw new AppException("MSG05", "The selected product was not found.", 400);
                }

                poItems.Add(new PurchaseOrderItem
                {
                    ProductId = itemReq.ProductId,
                    Product = product,
                    OrderedQuantity = itemReq.OrderedQuantity,
                    ReceivedQuantity = 0,
                    UnitPrice = itemReq.UnitPrice
                });
            }

            var code = await GenerateOrderCodeAsync();

            var po = new PurchaseOrder
            {
                PurchaseOrderCode = code,
                SupplierId = request.SupplierId,
                Supplier = supplier,
                CreatedBy = userId,
                Status = POStatus.Pending,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                CreatedAt = DateTime.UtcNow,
                Items = poItems
            };

            await _purchaseOrderRepository.AddAsync(po);

            var createdPo = await _purchaseOrderRepository.GetByIdAsync(po.PurchaseOrderId) ?? po;
            return MapToResponse(createdPo);
        }

        public async Task<PurchaseOrderResponse> ApproveOrderAsync(int id, int managerId)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            _poStatusService.ValidateTransition(order.Status, POStatus.Approved);

            order.Status = POStatus.Approved;
            order.ApprovedBy = managerId;
            order.ApprovedDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _purchaseOrderRepository.UpdateAsync(order);

            var updatedOrder = await _purchaseOrderRepository.GetByIdAsync(order.PurchaseOrderId) ?? order;
            return MapToResponse(updatedOrder);
        }

        public async Task<PurchaseOrderResponse> RejectOrderAsync(int id, int managerId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new AppException("MSG01", "Required information is missing.", 400);
            }

            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            _poStatusService.ValidateTransition(order.Status, POStatus.Rejected);

            order.Status = POStatus.Rejected;
            order.ApprovedBy = managerId;
            order.RejectionReason = reason;
            order.UpdatedAt = DateTime.UtcNow;

            await _purchaseOrderRepository.UpdateAsync(order);

            var updatedOrder = await _purchaseOrderRepository.GetByIdAsync(order.PurchaseOrderId) ?? order;
            return MapToResponse(updatedOrder);
        }

        public async Task<List<PurchaseOrderListResponse>> GetAllAsync(POStatus? status = null)
        {
            var orders = await _purchaseOrderRepository.GetAllAsync(status);
            return orders.Select(MapToListResponse).ToList();
        }

        public async Task<List<PurchaseOrderListResponse>> GetPendingOrdersAsync()
        {
            var orders = await _purchaseOrderRepository.GetByStatusAsync(POStatus.Pending);
            return orders.Select(MapToListResponse).ToList();
        }

        public async Task<PurchaseOrderResponse> GetOrderDetailAsync(int id)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new AppException("NOT_FOUND", "Purchase order not found.", 404);
            }

            return MapToResponse(order);
        }

        public async Task<PurchaseOrderFormDataResponse> GetFormDataAsync()
        {
            var suppliers = await _supplierRepository.GetActiveSuppliersAsync();
            var products = await _productRepository.GetActiveProductsAsync();
            var categories = _context != null
                ? await _context.Categories.ToListAsync()
                : new List<Category>();

            return new PurchaseOrderFormDataResponse
            {
                Suppliers = suppliers.Select(s => new SupplierOptionDto
                {
                    SupplierId = s.SupplierId,
                    SupplierCode = s.SupplierCode,
                    SupplierName = s.SupplierName,
                    Status = s.Status
                }).ToList(),
                Products = products.Select(p => new ProductOptionDto
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitPrice = p.UnitPrice,
                    MinimumStockLevel = p.MinimumStockLevel,
                    Status = p.Status
                }).ToList(),
                Categories = categories.Select(c => new CategoryOptionDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList()
            };
        }

        public async Task<ProductOptionDto> QuickCreateProductAsync(QuickCreateProductRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductName))
            {
                throw new AppException("MSG01", "Tên sản phẩm không được để trống.", 400);
            }

            if (request.UnitPrice < 0)
            {
                throw new AppException("MSG01", "Đơn giá không thể là số âm.", 400);
            }

            var categoryId = request.CategoryId.GetValueOrDefault(1);
            if (_context != null && !await _context.Categories.AnyAsync(c => c.CategoryId == categoryId))
            {
                categoryId = 1;
            }

            string code = request.ProductCode?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(code))
            {
                var randomNum = new Random().Next(1000, 9999);
                code = $"PRD-{randomNum}";
                while (await _productRepository.GetByCodeAsync(code) != null)
                {
                    randomNum = new Random().Next(1000, 9999);
                    code = $"PRD-{randomNum}";
                }
            }
            else
            {
                var existing = await _productRepository.GetByCodeAsync(code);
                if (existing != null)
                {
                    throw new AppException("MSG01", $"Mã sản phẩm '{code}' đã tồn tại trong hệ thống.", 400);
                }
            }

            var product = new Product
            {
                ProductCode = code,
                ProductName = request.ProductName.Trim(),
                CategoryId = categoryId,
                Brand = string.IsNullOrWhiteSpace(request.Brand) ? "Khác" : request.Brand.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? request.ProductName.Trim() : request.Description.Trim(),
                UnitPrice = request.UnitPrice,
                MinimumStockLevel = 5,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);

            if (_context != null)
            {
                var warehouses = await _context.Warehouses.ToListAsync();
                foreach (var wh in warehouses)
                {
                    if (!await _context.Inventories.AnyAsync(i => i.ProductId == product.ProductId && i.WarehouseId == wh.WarehouseId))
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            ProductId = product.ProductId,
                            WarehouseId = wh.WarehouseId,
                            Quantity = 0,
                            LastUpdatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return new ProductOptionDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                MinimumStockLevel = product.MinimumStockLevel,
                Status = product.Status
            };
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            int count = await _purchaseOrderRepository.GetCountAsync() + 1;
            string code = $"PO-{dateStr}-{count:D4}";

            while (await _purchaseOrderRepository.ExistsCodeAsync(code))
            {
                count++;
                code = $"PO-{dateStr}-{count:D4}";
            }

            return code;
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
