using ewms_common.Authentications;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Enums;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Repository.OutboundOrder.Request;
using ewms_service.DTOs.Repository.OutboundOrder.Response;
using ewms_service.Services.AccessLog;
using ewms_service.Utils;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Repository
{
    public interface IOutboundOrderService
    {
        Task<ExtendedPageResponse<OutboundOrderTableView>> SearchAsync(OutboundOrderTableFilter filter, UserHeader currentUser);
        Task<OutboundOrderDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(OutboundOrderCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ApproveAsync(OutboundOrderApproveRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> RejectAsync(OutboundOrderRejectRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> CancelAsync(OutboundOrderCancelRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> IssueGoodsAsync(OutboundOrderIssueRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class OutboundOrderService : IOutboundOrderService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public OutboundOrderService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<OutboundOrderTableView>> SearchAsync(OutboundOrderTableFilter filter, UserHeader currentUser)
        {
            var orders = await _context.outbound_orders
                .AsNoTracking()
                .Include(o => o.customer)
                .Include(o => o.warehouse)
                .Include(o => o.approver)
                .OrderByDescending(o => o.created_on)
                .Select(o => new OutboundOrderTableView
                {
                    id = o.id,
                    order_code = o.order_code,
                    customer_id = o.customer_id,
                    customer_code = o.customer != null ? o.customer.customer_code : string.Empty,
                    customer_name = o.customer != null ? o.customer.customer_name : string.Empty,
                    warehouse_id = o.warehouse_id,
                    warehouse_code = o.warehouse != null ? o.warehouse.warehouse_code : string.Empty,
                    warehouse_name = o.warehouse != null ? o.warehouse.warehouse_name : string.Empty,
                    order_date = o.order_date,
                    approved_date = o.approved_date,
                    completed_date = o.completed_date,
                    approved_by = o.approved_by,
                    approver_name = o.approver != null ? o.approver.full_name : null,
                    total_quantity = o.total_quantity,
                    total_amount = o.total_amount,
                    status = (int)o.status,
                    status_name = o.status.GetDescription(),
                    rejection_reason = o.rejection_reason,
                    note = o.note,
                    created_on = o.created_on,
                    created_by = o.created_by,
                    modified_on = o.modified_on,
                    modified_by = o.modified_by
                })
                .ToListAsync();

            var query = orders.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<OutboundOrderTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<OutboundOrderDetailView> GetByIdAsync(string id)
        {
            var o = await _context.outbound_orders
                .AsNoTracking()
                .Include(x => x.customer)
                .Include(x => x.warehouse)
                .Include(x => x.approver)
                .Include(x => x.outbound_order_details)
                    .ThenInclude(d => d.product)
                        .ThenInclude(p => p!.inventories)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.OutboundOrderNotFound);

            return new OutboundOrderDetailView
            {
                id = o.id,
                order_code = o.order_code,
                customer_id = o.customer_id,
                customer_code = o.customer?.customer_code ?? string.Empty,
                customer_name = o.customer?.customer_name ?? string.Empty,
                warehouse_id = o.warehouse_id,
                warehouse_code = o.warehouse?.warehouse_code ?? string.Empty,
                warehouse_name = o.warehouse?.warehouse_name ?? string.Empty,
                order_date = o.order_date,
                approved_date = o.approved_date,
                completed_date = o.completed_date,
                approved_by = o.approved_by,
                approver_name = o.approver?.full_name,
                total_quantity = o.total_quantity,
                total_amount = o.total_amount,
                status = (int)o.status,
                status_name = o.status.GetDescription(),
                rejection_reason = o.rejection_reason,
                note = o.note,
                created_on = o.created_on,
                created_by = o.created_by,
                modified_on = o.modified_on,
                modified_by = o.modified_by,
                items = o.outbound_order_details.Select(d => new OutboundOrderItemView
                {
                    id = d.id,
                    product_id = d.product_id,
                    product_code = d.product?.product_code ?? string.Empty,
                    product_name = d.product?.product_name ?? string.Empty,
                    brand = d.product?.brand,
                    quantity = d.quantity,
                    issued_quantity = d.issued_quantity,
                    current_stock_in_warehouse = d.product?.inventories
                        .Where(inv => inv.warehouse_id == o.warehouse_id)
                        .Sum(inv => inv.quantity) ?? 0,
                    unit_price = d.unit_price
                }).ToList()
            };
        }

        public async Task<string> CreateAsync(OutboundOrderCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, request.warehouse_id);

            if (request.items == null || !request.items.Any())
            {
                throw new BadException(ErrorMessage.OrderItemsEmpty);
            }

            var customerExists = await _context.customers.AsNoTracking().AnyAsync(c => c.id == request.customer_id && c.status == eStatus_Common.Active);
            if (!customerExists)
            {
                throw new NotFoundException(ErrorMessage.CustomerNotFound);
            }

            var groupedItems = request.items
                .GroupBy(i => i.product_id)
                .Select(g => new OutboundOrderItemRequest
                {
                    product_id = g.Key,
                    quantity = g.Sum(x => x.quantity),
                    unit_price = g.Last().unit_price
                })
                .ToList();

            var productIds = groupedItems.Select(i => i.product_id).ToList();
            var products = await _context.products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.id))
                .ToDictionaryAsync(p => p.id);

            if (products.Count != productIds.Count)
            {
                throw new NotFoundException(ErrorMessage.ProductNotFound);
            }

            var code = string.IsNullOrWhiteSpace(request.order_code)
                ? await GenerateOutboundOrderCodeAsync()
                : request.order_code.Trim().ToUpper();

            var totalQty = groupedItems.Sum(i => i.quantity);
            var totalAmount = groupedItems.Sum(i => i.quantity * (i.unit_price ?? products[i.product_id].unit_price));

            var order = new outbound_order
            {
                order_code = code,
                customer_id = request.customer_id,
                warehouse_id = request.warehouse_id,
                order_date = request.order_date ?? DateTime.Now,
                status = eStatus_OutboundOrder.Pending,
                total_quantity = totalQty,
                total_amount = totalAmount,
                note = request.note?.Trim()
            }.Created(currentUser.user_id);

            await _context.outbound_orders.AddAsync(order);

            foreach (var item in groupedItems)
            {
                var unitPrice = item.unit_price ?? products[item.product_id].unit_price;
                await _context.outbound_order_details.AddAsync(new outbound_order_detail
                {
                    order_id = order.id,
                    product_id = item.product_id,
                    quantity = item.quantity,
                    issued_quantity = 0,
                    unit_price = unitPrice
                }.Created(currentUser.user_id));
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "OUTBOUND_ORDER",
                action = "CREATE",
                entity_type = "outbound_orders",
                entity_id = order.id,
                description = $"Tạo đơn xuất kho '{order.order_code}' (Tổng SL: {order.total_quantity}, Tổng tiền: {order.total_amount:N0} VNĐ).",
                ip_address = ipAddress
            });

            return order.id;
        }

        public async Task<bool> ApproveAsync(OutboundOrderApproveRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var order = await _context.outbound_orders
                .Include(o => o.outbound_order_details)
                    .ThenInclude(d => d.product)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.OutboundOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, order.warehouse_id);

            if (order.status != eStatus_OutboundOrder.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, order.status.GetDescription()));
            }

            // Check sufficient stock before approving
            foreach (var detail in order.outbound_order_details)
            {
                var inv = await _context.inventories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.warehouse_id == order.warehouse_id && i.product_id == detail.product_id);
                var currentQty = inv?.quantity ?? 0;
                if (currentQty < detail.quantity)
                {
                    var prodName = detail.product?.product_name ?? detail.product_id;
                    throw new BadException(string.Format(ErrorMessage.MSG05_InsufficientStock, prodName, currentQty, detail.quantity));
                }
            }

            order.status = eStatus_OutboundOrder.Approved;
            order.approved_by = currentUser.user_id;
            order.approved_date = DateTime.Now;
            order.Modified(currentUser.user_id);

            _context.outbound_orders.Update(order);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "OUTBOUND_ORDER",
                action = "APPROVE",
                entity_type = "outbound_orders",
                entity_id = order.id,
                description = $"Phê duyệt đơn xuất kho '{order.order_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> RejectAsync(OutboundOrderRejectRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(request.rejection_reason))
            {
                throw new BadException(ErrorMessage.RejectionReasonRequired);
            }

            var order = await _context.outbound_orders.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.OutboundOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, order.warehouse_id);

            if (order.status != eStatus_OutboundOrder.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, order.status.GetDescription()));
            }

            order.status = eStatus_OutboundOrder.Rejected;
            order.approved_by = currentUser.user_id;
            order.approved_date = DateTime.Now;
            order.rejection_reason = request.rejection_reason.Trim();
            order.Modified(currentUser.user_id);

            _context.outbound_orders.Update(order);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "OUTBOUND_ORDER",
                action = "REJECT",
                entity_type = "outbound_orders",
                entity_id = order.id,
                description = $"Từ chối đơn xuất kho '{order.order_code}'. Lý do: {order.rejection_reason}",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> CancelAsync(OutboundOrderCancelRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var order = await _context.outbound_orders.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.OutboundOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, order.warehouse_id);

            if (order.status != eStatus_OutboundOrder.Pending && order.status != eStatus_OutboundOrder.Approved)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, order.status.GetDescription()));
            }

            order.status = eStatus_OutboundOrder.Cancelled;
            order.Modified(currentUser.user_id);

            _context.outbound_orders.Update(order);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "OUTBOUND_ORDER",
                action = "CANCEL",
                entity_type = "outbound_orders",
                entity_id = order.id,
                description = $"Hủy đơn xuất kho '{order.order_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> IssueGoodsAsync(OutboundOrderIssueRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var order = await _context.outbound_orders
                .Include(o => o.outbound_order_details)
                    .ThenInclude(d => d.product)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.OutboundOrderNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, order.warehouse_id);

            if (order.status != eStatus_OutboundOrder.Approved)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, order.status.GetDescription()));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in order.outbound_order_details)
                {
                    var inv = await _context.inventories
                        .FirstOrDefaultAsync(i => i.warehouse_id == order.warehouse_id && i.product_id == detail.product_id);

                    var currentQty = inv?.quantity ?? 0;
                    if (inv == null || currentQty < detail.quantity)
                    {
                        var prodName = detail.product?.product_name ?? detail.product_id;
                        throw new BadException(string.Format(ErrorMessage.MSG05_InsufficientStock, prodName, currentQty, detail.quantity));
                    }

                    inv.quantity -= detail.quantity;
                    inv.last_updated_at = DateTime.Now;
                    inv.Modified(currentUser.user_id);
                    _context.inventories.Update(inv);

                    detail.issued_quantity = detail.quantity;
                    detail.Modified(currentUser.user_id);
                    _context.outbound_order_details.Update(detail);

                    await _context.inventory_transactions.AddAsync(new inventory_transaction
                    {
                        warehouse_id = order.warehouse_id,
                        product_id = detail.product_id,
                        user_id = currentUser.user_id,
                        outbound_order_detail_id = detail.id,
                        transaction_type = eType_InventoryTransaction.Outbound,
                        quantity = -detail.quantity,
                        transaction_date = DateTime.Now,
                        note = request.note ?? $"Xuất kho theo đơn xuất {order.order_code}"
                    }.Created(currentUser.user_id));
                }

                order.status = eStatus_OutboundOrder.Completed;
                order.completed_date = DateTime.Now;
                order.Modified(currentUser.user_id);
                _context.outbound_orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _accessLogService.CreateAsync(new AccessLogCreating
                {
                    user_id = currentUser.user_id,
                    module = "OUTBOUND_ORDER",
                    action = "ISSUE_GOODS",
                    entity_type = "outbound_orders",
                    entity_id = order.id,
                    description = $"Thực xuất kho đơn xuất '{order.order_code}' -> Hoàn thành.",
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

        private async Task<string> GenerateOutboundOrderCodeAsync()
        {
            var count = await _context.outbound_orders.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.OutboundOrder}-{DateTime.Now:yyyyMM}-{(count + 1):D4}";
        }
    }

    public static class OutboundOrderServiceExtension
    {
        public static IQueryable<OutboundOrderTableView> GetQueryable(
            this IQueryable<OutboundOrderTableView> query,
            OutboundOrderTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.order_code.ToLower().Contains(kw) ||
                    e.customer_name.ToLower().Contains(kw) ||
                    e.warehouse_name.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.customer_id))
            {
                query = query.Where(e => e.customer_id == filter.customer_id);
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
