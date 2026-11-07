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
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Repository.Inventory.Request;
using ewms_service.DTOs.Repository.Inventory.Response;
using ewms_service.Services.AccessLog;
using ewms_service.Utils;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Repository
{
    public interface IInventoryService
    {
        Task<ExtendedPageResponse<InventoryTableView>> SearchInventoryAsync(InventoryTableFilter filter, UserHeader currentUser);
        Task<bool> AdjustInventoryAsync(InventoryAdjustmentRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<ExtendedPageResponse<InventoryTransactionTableView>> SearchTransactionsAsync(InventoryTransactionTableFilter filter, UserHeader currentUser);
        Task<DashboardSummaryView> GetDashboardSummaryAsync(string? warehouseId, UserHeader currentUser);
    }

    public class InventoryService : IInventoryService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public InventoryService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<InventoryTableView>> SearchInventoryAsync(InventoryTableFilter filter, UserHeader currentUser)
        {
            var inventories = await _context.inventories
                .AsNoTracking()
                .Include(i => i.warehouse)
                .Include(i => i.product)
                    .ThenInclude(p => p!.category)
                .OrderByDescending(i => i.last_updated_at)
                .Select(i => new InventoryTableView
                {
                    id = i.id,
                    warehouse_id = i.warehouse_id,
                    warehouse_code = i.warehouse != null ? i.warehouse.warehouse_code : string.Empty,
                    warehouse_name = i.warehouse != null ? i.warehouse.warehouse_name : string.Empty,
                    product_id = i.product_id,
                    product_code = i.product != null ? i.product.product_code : string.Empty,
                    product_name = i.product != null ? i.product.product_name : string.Empty,
                    category_id = i.product != null ? i.product.category_id : string.Empty,
                    category_name = i.product != null && i.product.category != null ? i.product.category.category_name : string.Empty,
                    brand = i.product != null ? i.product.brand : null,
                    unit_price = i.product != null ? i.product.unit_price : 0,
                    quantity = i.quantity,
                    minimum_stock_level = i.product != null ? i.product.minimum_stock_level : 0,
                    is_low_stock = i.product != null && i.quantity <= i.product.minimum_stock_level,
                    last_updated_at = i.last_updated_at,
                    created_on = i.created_on,
                    created_by = i.created_by,
                    modified_on = i.modified_on,
                    modified_by = i.modified_by
                })
                .ToListAsync();

            var query = inventories.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<InventoryTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<bool> AdjustInventoryAsync(InventoryAdjustmentRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, request.warehouse_id);

            var product = await _context.products.AsNoTracking().FirstOrDefaultAsync(p => p.id == request.product_id)
                ?? throw new NotFoundException(ErrorMessage.ProductNotFound);

            var inv = await _context.inventories
                .FirstOrDefaultAsync(i => i.warehouse_id == request.warehouse_id && i.product_id == request.product_id);

            int diff;
            if (inv == null)
            {
                diff = request.new_quantity;
                inv = new inventory
                {
                    warehouse_id = request.warehouse_id,
                    product_id = request.product_id,
                    quantity = request.new_quantity,
                    last_updated_at = DateTime.Now
                }.Created(currentUser.user_id);
                await _context.inventories.AddAsync(inv);
            }
            else
            {
                diff = request.new_quantity - inv.quantity;
                inv.quantity = request.new_quantity;
                inv.last_updated_at = DateTime.Now;
                inv.Modified(currentUser.user_id);
                _context.inventories.Update(inv);
            }

            if (diff != 0)
            {
                await _context.inventory_transactions.AddAsync(new inventory_transaction
                {
                    warehouse_id = request.warehouse_id,
                    product_id = request.product_id,
                    user_id = currentUser.user_id,
                    transaction_type = eType_InventoryTransaction.Adjustment,
                    quantity = diff,
                    transaction_date = DateTime.Now,
                    note = request.note.Trim()
                }.Created(currentUser.user_id));
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "INVENTORY",
                action = "ADJUST",
                entity_type = "inventories",
                entity_id = inv.id,
                description = $"Điều chỉnh tồn kho sản phẩm '{product.product_code}' -> SL mới: {request.new_quantity} (Chênh lệch: {diff:+#;-#;0}). Lý do: {request.note}",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<ExtendedPageResponse<InventoryTransactionTableView>> SearchTransactionsAsync(InventoryTransactionTableFilter filter, UserHeader currentUser)
        {
            var transactions = await _context.inventory_transactions
                .AsNoTracking()
                .Include(t => t.warehouse)
                .Include(t => t.product)
                .Include(t => t.user)
                .Include(t => t.purchase_order_item)
                    .ThenInclude(poi => poi!.purchase_order)
                .Include(t => t.outbound_order_detail)
                    .ThenInclude(ood => ood!.outbound_order)
                .Include(t => t.stock_transfer)
                .OrderByDescending(t => t.transaction_date)
                .ToListAsync();

            var items = transactions.Select(t =>
            {
                var refCode = t.purchase_order_item?.purchase_order?.purchase_order_code
                    ?? t.outbound_order_detail?.outbound_order?.order_code
                    ?? t.stock_transfer?.stock_transfer_code;

                return new InventoryTransactionTableView
                {
                    id = t.id,
                    warehouse_id = t.warehouse_id,
                    warehouse_code = t.warehouse?.warehouse_code ?? string.Empty,
                    warehouse_name = t.warehouse?.warehouse_name ?? string.Empty,
                    product_id = t.product_id,
                    product_code = t.product?.product_code ?? string.Empty,
                    product_name = t.product?.product_name ?? string.Empty,
                    user_id = t.user_id,
                    employee_code = t.user?.employee_code ?? string.Empty,
                    user_full_name = t.user?.full_name ?? string.Empty,
                    transaction_type = (int)t.transaction_type,
                    transaction_type_name = t.transaction_type.GetDescription(),
                    quantity = t.quantity,
                    transaction_date = t.transaction_date,
                    reference_code = refCode,
                    note = t.note,
                    created_on = t.created_on,
                    created_by = t.created_by,
                    modified_on = t.modified_on,
                    modified_by = t.modified_by
                };
            }).AsQueryable().GetQueryable(filter);

            return new ExtendedPageResponse<InventoryTransactionTableView>
            {
                Items = items.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = items.Count()
            };
        }

        public async Task<DashboardSummaryView> GetDashboardSummaryAsync(string? warehouseId, UserHeader currentUser)
        {
            var invQuery = _context.inventories
                .AsNoTracking()
                .Include(i => i.warehouse)
                .Include(i => i.product)
                    .ThenInclude(p => p!.category)
                .AsQueryable();

            var poQuery = _context.purchase_orders.AsNoTracking().AsQueryable();
            var soQuery = _context.outbound_orders.AsNoTracking().AsQueryable();
            var trfQuery = _context.stock_transfers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(warehouseId))
            {
                invQuery = invQuery.Where(i => i.warehouse_id == warehouseId);
                poQuery = poQuery.Where(po => po.warehouse_id == warehouseId);
                soQuery = soQuery.Where(so => so.warehouse_id == warehouseId);
                trfQuery = trfQuery.Where(t => t.source_warehouse_id == warehouseId || t.destination_warehouse_id == warehouseId);
            }

            var invList = await invQuery.ToListAsync();
            var lowStockItems = invList
                .Where(i => i.product != null && i.quantity <= i.product.minimum_stock_level)
                .Select(i => new InventoryTableView
                {
                    id = i.id,
                    warehouse_id = i.warehouse_id,
                    warehouse_code = i.warehouse?.warehouse_code ?? string.Empty,
                    warehouse_name = i.warehouse?.warehouse_name ?? string.Empty,
                    product_id = i.product_id,
                    product_code = i.product?.product_code ?? string.Empty,
                    product_name = i.product?.product_name ?? string.Empty,
                    category_id = i.product?.category_id ?? string.Empty,
                    category_name = i.product?.category?.category_name ?? string.Empty,
                    brand = i.product?.brand,
                    unit_price = i.product?.unit_price ?? 0,
                    quantity = i.quantity,
                    minimum_stock_level = i.product?.minimum_stock_level ?? 0,
                    is_low_stock = true,
                    last_updated_at = i.last_updated_at
                })
                .OrderBy(i => i.quantity)
                .Take(10)
                .ToList();

            var recentTx = await SearchTransactionsAsync(new InventoryTransactionTableFilter
            {
                warehouse_id = warehouseId,
                page_index = 1,
                page_size = 10
            }, currentUser);

            return new DashboardSummaryView
            {
                total_products = await _context.products.CountAsync(),
                total_warehouses = await _context.warehouses.CountAsync(w => w.status == eStatus_Warehouse.Active),
                total_suppliers = await _context.suppliers.CountAsync(s => s.status == eStatus_Supplier.Active),
                total_customers = await _context.customers.CountAsync(c => c.status == eStatus_Common.Active),
                total_stock_units = invList.Sum(i => i.quantity),
                total_inventory_value = invList.Sum(i => i.quantity * (i.product?.unit_price ?? 0)),
                low_stock_alert_count = invList.Count(i => i.product != null && i.quantity <= i.product.minimum_stock_level),
                pending_purchase_orders = await poQuery.CountAsync(po => po.status == eStatus_PurchaseOrder.Pending || po.status == eStatus_PurchaseOrder.Approved || po.status == eStatus_PurchaseOrder.PartiallyReceived),
                pending_outbound_orders = await soQuery.CountAsync(so => so.status == eStatus_OutboundOrder.Pending || so.status == eStatus_OutboundOrder.Approved),
                pending_stock_transfers = await trfQuery.CountAsync(t => t.status == eStatus_StockTransfer.Pending || t.status == eStatus_StockTransfer.Approved || t.status == eStatus_StockTransfer.InTransit),
                low_stock_items = lowStockItems,
                recent_transactions = recentTx.Items.ToList()
            };
        }
    }

    public static class InventoryServiceExtension
    {
        public static IQueryable<InventoryTableView> GetQueryable(
            this IQueryable<InventoryTableView> query,
            InventoryTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.product_code.ToLower().Contains(kw) ||
                    e.product_name.ToLower().Contains(kw) ||
                    e.warehouse_name.ToLower().Contains(kw) ||
                    (e.brand != null && e.brand.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.warehouse_id))
            {
                query = query.Where(e => e.warehouse_id == filter.warehouse_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.category_id))
            {
                query = query.Where(e => e.category_id == filter.category_id);
            }

            if (filter.low_stock_only == true)
            {
                query = query.Where(e => e.is_low_stock);
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

        public static IQueryable<InventoryTransactionTableView> GetQueryable(
            this IQueryable<InventoryTransactionTableView> query,
            InventoryTransactionTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.product_code.ToLower().Contains(kw) ||
                    e.product_name.ToLower().Contains(kw) ||
                    e.user_full_name.ToLower().Contains(kw) ||
                    (e.reference_code != null && e.reference_code.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.warehouse_id))
            {
                query = query.Where(e => e.warehouse_id == filter.warehouse_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.product_id))
            {
                query = query.Where(e => e.product_id == filter.product_id);
            }

            if (filter.transaction_type != null)
            {
                query = query.Where(e => e.transaction_type == (int)filter.transaction_type.Value);
            }

            if (filter.from_date != null)
            {
                query = query.Where(e => e.transaction_date.Date >= filter.from_date.Value.Date);
            }

            if (filter.to_date != null)
            {
                query = query.Where(e => e.transaction_date.Date <= filter.to_date.Value.Date);
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
