using ewms_common.Authentications;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Enums;
using ewms_infra.Models.Repository;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Repository.PurchaseOrder.Request;
using ewms_service.DTOs.Repository.PurchaseOrder.Response;
using ewms_service.Services.AccessLog;
using ewms_service.Utils;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Repository
{
    public interface IPurchaseOrderService
    {
        Task<ExtendedPageResponse<PurchaseOrderTableView>> SearchAsync(PurchaseOrderTableFilter filter, UserHeader currentUser);
        Task<PurchaseOrderDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(PurchaseOrderCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ApproveAsync(PurchaseOrderApproveRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> RejectAsync(PurchaseOrderRejectRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> CancelAsync(PurchaseOrderCancelRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ReceiveGoodsAsync(PurchaseOrderReceiveRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public PurchaseOrderService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<PurchaseOrderTableView>> SearchAsync(PurchaseOrderTableFilter filter, UserHeader currentUser)
        {
            var orders = await _context.purchase_orders
                .AsNoTracking()
                .Include(po => po.supplier)
                .Include(po => po.warehouse)
                .Include(po => po.approver)
                .Include(po => po.purchase_order_items)
                .OrderByDescending(po => po.created_on)
                .Select(po => new PurchaseOrderTableView
                {
                    id = po.id,
                    purchase_order_code = po.purchase_order_code,
                    supplier_id = po.supplier_id,
                    supplier_code = po.supplier != null ? po.supplier.supplier_code : string.Empty,
                    supplier_name = po.supplier != null ? po.supplier.supplier_name : string.Empty,
                    warehouse_id = po.warehouse_id,
                    warehouse_code = po.warehouse != null ? po.warehouse.warehouse_code : string.Empty,
                    warehouse_name = po.warehouse != null ? po.warehouse.warehouse_name : string.Empty,
                    order_date = po.order_date,
                    expected_delivery_date = po.expected_delivery_date,
                    approved_date = po.approved_date,
                    completed_date = po.completed_date,
                    approved_by = po.approved_by,
                    approver_name = po.approver != null ? po.approver.full_name : null,
                    total_ordered_quantity = po.purchase_order_items.Sum(i => i.ordered_quantity),
                    total_received_quantity = po.purchase_order_items.Sum(i => i.received_quantity),
                    total_amount = po.total_amount,
                    status = (int)po.status,
                    status_name = po.status.GetDescription(),
                    rejection_reason = po.rejection_reason,
                    note = po.note,
                    created_on = po.created_on,
                    created_by = po.created_by,
                    modified_on = po.modified_on,
                    modified_by = po.modified_by
                })
                .ToListAsync();

            var query = orders.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<PurchaseOrderTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<PurchaseOrderDetailView> GetByIdAsync(string id)
        {
            var po = await _context.purchase_orders
                .AsNoTracking()
                .Include(x => x.supplier)
                .Include(x => x.warehouse)
                .Include(x => x.approver)
                .Include(x => x.purchase_order_items)
                    .ThenInclude(i => i.product)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.PurchaseOrderNotFound);

            return new PurchaseOrderDetailView
            {
                id = po.id,
                purchase_order_code = po.purchase_order_code,
                supplier_id = po.supplier_id,
                supplier_code = po.supplier?.supplier_code ?? string.Empty,
                supplier_name = po.supplier?.supplier_name ?? string.Empty,
                warehouse_id = po.warehouse_id,
                warehouse_code = po.warehouse?.warehouse_code ?? string.Empty,
                warehouse_name = po.warehouse?.warehouse_name ?? string.Empty,
                order_date = po.order_date,
                expected_delivery_date = po.expected_delivery_date,
                approved_date = po.approved_date,
                completed_date = po.completed_date,
                approved_by = po.approved_by,
                approver_name = po.approver?.full_name,
                total_ordered_quantity = po.purchase_order_items.Sum(i => i.ordered_quantity),
                total_received_quantity = po.purchase_order_items.Sum(i => i.received_quantity),
                total_amount = po.total_amount,
                status = (int)po.status,
                status_name = po.status.GetDescription(),
                rejection_reason = po.rejection_reason,
                note = po.note,
                created_on = po.created_on,
                created_by = po.created_by,
                modified_on = po.modified_on,
                modified_by = po.modified_by,
                items = po.purchase_order_items.Select(i => new PurchaseOrderItemView
                {
                    id = i.id,
                    product_id = i.product_id,
                    product_code = i.product?.product_code ?? string.Empty,
                    product_name = i.product?.product_name ?? string.Empty,
                    brand = i.product?.brand,
                    ordered_quantity = i.ordered_quantity,
                    received_quantity = i.received_quantity,
                    unit_price = i.unit_price
                }).ToList()
            };
        }

        public async Task<string> CreateAsync(PurchaseOrderCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, request.warehouse_id);
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.purchase_order_code)
                ? await GeneratePurchaseOrderCodeAsync()
                : request.purchase_order_code.Trim().ToUpper();

            var groupedItems = request.items
                .GroupBy(i => i.product_id)
                .Select(g => new PurchaseOrderItemRequest
                {
                    product_id = g.Key,
                    ordered_quantity = g.Sum(x => x.ordered_quantity),
                    unit_price = g.Last().unit_price
                })
                .ToList();

            var totalAmount = groupedItems.Sum(i => i.ordered_quantity * i.unit_price);

            var po = new purchase_order
            {
                purchase_order_code = code,
                supplier_id = request.supplier_id,
                warehouse_id = request.warehouse_id,
                status = eStatus_PurchaseOrder.Pending,
                order_date = request.order_date ?? DateTime.Now,
                expected_delivery_date = request.expected_delivery_date,
                total_amount = totalAmount,
                note = request.note?.Trim()
            }.Created(currentUser.user_id);

            await _context.purchase_orders.AddAsync(po);

            foreach (var item in groupedItems)
            {
                await _context.purchase_order_items.AddAsync(new purchase_order_item
                {
                    purchase_order_id = po.id,
                    product_id = item.product_id,
                    ordered_quantity = item.ordered_quantity,
                    received_quantity = 0,
                    unit_price = item.unit_price
                }.Created(currentUser.user_id));
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PURCHASE_ORDER",
                action = "CREATE",
                entity_type = "purchase_orders",
                entity_id = po.id,
                description = $"Tạo đơn mua hàng '{po.purchase_order_code}' (Tổng tiền: {po.total_amount:N0} VNĐ).",
                ip_address = ipAddress
            });

            return po.id;
        }

        public async Task<bool> ApproveAsync(PurchaseOrderApproveRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var po = await _context.purchase_orders.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.PurchaseOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, po.warehouse_id);

            if (po.status != eStatus_PurchaseOrder.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, po.status.GetDescription()));
            }

            po.status = eStatus_PurchaseOrder.Approved;
            po.approved_by = currentUser.user_id;
            po.approved_date = DateTime.Now;
            po.Modified(currentUser.user_id);

            _context.purchase_orders.Update(po);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PURCHASE_ORDER",
                action = "APPROVE",
                entity_type = "purchase_orders",
                entity_id = po.id,
                description = $"Phê duyệt đơn mua hàng '{po.purchase_order_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> RejectAsync(PurchaseOrderRejectRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(request.rejection_reason))
            {
                throw new BadException(ErrorMessage.RejectionReasonRequired);
            }

            var po = await _context.purchase_orders.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.PurchaseOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, po.warehouse_id);

            if (po.status != eStatus_PurchaseOrder.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, po.status.GetDescription()));
            }

            po.status = eStatus_PurchaseOrder.Rejected;
            po.approved_by = currentUser.user_id;
            po.approved_date = DateTime.Now;
            po.rejection_reason = request.rejection_reason.Trim();
            po.Modified(currentUser.user_id);

            _context.purchase_orders.Update(po);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PURCHASE_ORDER",
                action = "REJECT",
                entity_type = "purchase_orders",
                entity_id = po.id,
                description = $"Từ chối đơn mua hàng '{po.purchase_order_code}'. Lý do: {po.rejection_reason}",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> CancelAsync(PurchaseOrderCancelRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var po = await _context.purchase_orders
                .Include(x => x.purchase_order_items)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.PurchaseOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, po.warehouse_id);

            if (po.status != eStatus_PurchaseOrder.Pending && po.status != eStatus_PurchaseOrder.Approved)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, po.status.GetDescription()));
            }

            if (po.purchase_order_items.Any(i => i.received_quantity > 0))
            {
                throw new BadException("Không thể hủy đơn mua hàng đã phát sinh nhập kho.");
            }

            po.status = eStatus_PurchaseOrder.Cancelled;
            po.Modified(currentUser.user_id);

            _context.purchase_orders.Update(po);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PURCHASE_ORDER",
                action = "CANCEL",
                entity_type = "purchase_orders",
                entity_id = po.id,
                description = $"Hủy đơn mua hàng '{po.purchase_order_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> ReceiveGoodsAsync(PurchaseOrderReceiveRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var po = await _context.purchase_orders
                .Include(x => x.purchase_order_items)
                    .ThenInclude(i => i.product)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.PurchaseOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, po.warehouse_id);

            if (po.status != eStatus_PurchaseOrder.Approved && po.status != eStatus_PurchaseOrder.PartiallyReceived)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, po.status.GetDescription()));
            }

            if (request.items == null || !request.items.Any())
            {
                throw new BadException(ErrorMessage.OrderItemsEmpty);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var receiveItem in request.items.Where(x => x.receive_quantity > 0))
                {
                    var poItem = po.purchase_order_items.FirstOrDefault(x => x.id == receiveItem.purchase_order_item_id)
                        ?? throw new NotFoundException("Không tìm thấy dòng sản phẩm trong đơn mua hàng.");

                    var remaining = poItem.ordered_quantity - poItem.received_quantity;
                    if (receiveItem.receive_quantity > remaining)
                    {
                        var prodName = poItem.product?.product_name ?? poItem.product_id;
                        throw new BadException(string.Format(
                            ErrorMessage.MSG06_ExceedRemainingQuantity,
                            prodName,
                            remaining,
                            receiveItem.receive_quantity));
                    }

                    poItem.received_quantity += receiveItem.receive_quantity;
                    poItem.Modified(currentUser.user_id);
                    _context.purchase_order_items.Update(poItem);

                    var inv = await _context.inventories
                        .FirstOrDefaultAsync(i => i.warehouse_id == po.warehouse_id && i.product_id == poItem.product_id);

                    if (inv == null)
                    {
                        inv = new inventory
                        {
                            warehouse_id = po.warehouse_id,
                            product_id = poItem.product_id,
                            quantity = receiveItem.receive_quantity,
                            last_updated_at = DateTime.Now
                        }.Created(currentUser.user_id);
                        await _context.inventories.AddAsync(inv);
                    }
                    else
                    {
                        inv.quantity += receiveItem.receive_quantity;
                        inv.last_updated_at = DateTime.Now;
                        inv.Modified(currentUser.user_id);
                        _context.inventories.Update(inv);
                    }

                    await _context.inventory_transactions.AddAsync(new inventory_transaction
                    {
                        warehouse_id = po.warehouse_id,
                        product_id = poItem.product_id,
                        user_id = currentUser.user_id,
                        purchase_order_item_id = poItem.id,
                        transaction_type = eType_InventoryTransaction.Inbound,
                        quantity = receiveItem.receive_quantity,
                        transaction_date = DateTime.Now,
                        note = request.note ?? $"Nhập kho từ đơn mua hàng {po.purchase_order_code}"
                    }.Created(currentUser.user_id));
                }

                var isFullyReceived = po.purchase_order_items.All(i => i.received_quantity >= i.ordered_quantity);
                po.status = isFullyReceived
                    ? eStatus_PurchaseOrder.Completed
                    : eStatus_PurchaseOrder.PartiallyReceived;

                if (isFullyReceived)
                {
                    po.completed_date = DateTime.Now;
                }

                po.Modified(currentUser.user_id);
                _context.purchase_orders.Update(po);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _accessLogService.CreateAsync(new AccessLogCreating
                {
                    user_id = currentUser.user_id,
                    module = "PURCHASE_ORDER",
                    action = "RECEIVE_GOODS",
                    entity_type = "purchase_orders",
                    entity_id = po.id,
                    description = $"Thực nhập kho đơn mua hàng '{po.purchase_order_code}' -> Trạng thái: {po.status.GetDescription()}.",
                    ip_address = ipAddress
                });

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GeneratePurchaseOrderCodeAsync()
        {
            var count = await _context.purchase_orders.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.PurchaseOrder}-{DateTime.Now:yyyyMM}-{(count + 1):D4}";
        }
    }

    public static class PurchaseOrderServiceExtension
    {
        public static async Task ValidateAsync(this PurchaseOrderBase request, ewms_context context)
        {
            if (request.items == null || !request.items.Any())
            {
                throw new BadException(ErrorMessage.OrderItemsEmpty);
            }

            var supplierExists = await context.suppliers.AsNoTracking().AnyAsync(s => s.id == request.supplier_id && s.status == eStatus_Supplier.Active);
            if (!supplierExists)
            {
                throw new NotFoundException(ErrorMessage.SupplierNotFound);
            }

            var productIds = request.items.Select(i => i.product_id).Distinct().ToList();
            var existingProducts = await context.products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.id))
                .Select(p => p.id)
                .ToListAsync();

            if (existingProducts.Count != productIds.Count)
            {
                throw new NotFoundException(ErrorMessage.ProductNotFound);
            }
        }

        public static IQueryable<PurchaseOrderTableView> GetQueryable(
            this IQueryable<PurchaseOrderTableView> query,
            PurchaseOrderTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.purchase_order_code.ToLower().Contains(kw) ||
                    e.supplier_name.ToLower().Contains(kw) ||
                    e.warehouse_name.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.supplier_id))
            {
                query = query.Where(e => e.supplier_id == filter.supplier_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.warehouse_id))
            {
                query = query.Where(e => e.warehouse_id == filter.warehouse_id);
            }

            if (filter.status != null)
            {
                query = query.Where(e => e.status == (int)filter.status.Value);
            }

            if (filter.from_date != null)
            {
                query = query.Where(e => e.order_date.Date >= filter.from_date.Value.Date);
            }

            if (filter.to_date != null)
            {
                query = query.Where(e => e.order_date.Date <= filter.to_date.Value.Date);
            }

            if (filter.filters != null && filter.filters.Any())
            {
                query = filter.filters.Aggregate(query, (current, item) => current.Filter(item));
            }

            if (filter.sort != null)
            {
                query = query.Sort(filter.sort);
            }

            return query;
        }
    }
}
