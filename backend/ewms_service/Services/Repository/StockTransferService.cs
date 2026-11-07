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
using ewms_infra.Models.Repository.StockTransfer;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Repository.StockTransfer.Request;
using ewms_service.DTOs.Repository.StockTransfer.Response;
using ewms_service.Services.AccessLog;
using ewms_service.Utils;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Repository
{
    public interface IStockTransferService
    {
        Task<ExtendedPageResponse<StockTransferTableView>> SearchAsync(StockTransferTableFilter filter, UserHeader currentUser);
        Task<StockTransferDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(StockTransferCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ApproveAsync(StockTransferApproveRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> RejectAsync(StockTransferRejectRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> CancelAsync(StockTransferCancelRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DispatchTransferAsync(StockTransferDispatchRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ReceiveTransferAsync(StockTransferReceiveRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class StockTransferService : IStockTransferService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public StockTransferService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<StockTransferTableView>> SearchAsync(StockTransferTableFilter filter, UserHeader currentUser)
        {
            var transfers = await _context.stock_transfers
                .AsNoTracking()
                .Include(st => st.source_warehouse)
                .Include(st => st.destination_warehouse)
                .Include(st => st.approver)
                .Include(st => st.dispatcher)
                .Include(st => st.receiver)
                .Include(st => st.stock_transfer_items)
                .OrderByDescending(st => st.created_on)
                .Select(st => new StockTransferTableView
                {
                    id = st.id,
                    stock_transfer_code = st.stock_transfer_code,
                    source_warehouse_id = st.source_warehouse_id,
                    source_warehouse_code = st.source_warehouse != null ? st.source_warehouse.warehouse_code : string.Empty,
                    source_warehouse_name = st.source_warehouse != null ? st.source_warehouse.warehouse_name : string.Empty,
                    destination_warehouse_id = st.destination_warehouse_id,
                    destination_warehouse_code = st.destination_warehouse != null ? st.destination_warehouse.warehouse_code : string.Empty,
                    destination_warehouse_name = st.destination_warehouse != null ? st.destination_warehouse.warehouse_name : string.Empty,
                    transfer_date = st.transfer_date,
                    approved_date = st.approved_date,
                    dispatched_date = st.dispatched_date,
                    completed_date = st.completed_date,
                    approved_by = st.approved_by,
                    approver_name = st.approver != null ? st.approver.full_name : null,
                    dispatched_by = st.dispatched_by,
                    dispatcher_name = st.dispatcher != null ? st.dispatcher.full_name : null,
                    received_by = st.received_by,
                    receiver_name = st.receiver != null ? st.receiver.full_name : null,
                    total_quantity = st.stock_transfer_items.Sum(i => i.quantity),
                    status = (int)st.status,
                    status_name = st.status.GetDescription(),
                    rejection_reason = st.rejection_reason,
                    cancel_reason = st.cancel_reason,
                    note = st.note,
                    created_on = st.created_on,
                    created_by = st.created_by,
                    modified_on = st.modified_on,
                    modified_by = st.modified_by
                })
                .ToListAsync();

            var query = transfers.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<StockTransferTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<StockTransferDetailView> GetByIdAsync(string id)
        {
            var st = await _context.stock_transfers
                .AsNoTracking()
                .Include(x => x.source_warehouse)
                .Include(x => x.destination_warehouse)
                .Include(x => x.approver)
                .Include(x => x.dispatcher)
                .Include(x => x.receiver)
                .Include(x => x.stock_transfer_items)
                    .ThenInclude(i => i.product)
                        .ThenInclude(p => p!.inventories)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            return new StockTransferDetailView
            {
                id = st.id,
                stock_transfer_code = st.stock_transfer_code,
                source_warehouse_id = st.source_warehouse_id,
                source_warehouse_code = st.source_warehouse?.warehouse_code ?? string.Empty,
                source_warehouse_name = st.source_warehouse?.warehouse_name ?? string.Empty,
                destination_warehouse_id = st.destination_warehouse_id,
                destination_warehouse_code = st.destination_warehouse?.warehouse_code ?? string.Empty,
                destination_warehouse_name = st.destination_warehouse?.warehouse_name ?? string.Empty,
                transfer_date = st.transfer_date,
                approved_date = st.approved_date,
                dispatched_date = st.dispatched_date,
                completed_date = st.completed_date,
                approved_by = st.approved_by,
                approver_name = st.approver?.full_name,
                dispatched_by = st.dispatched_by,
                dispatcher_name = st.dispatcher?.full_name,
                received_by = st.received_by,
                receiver_name = st.receiver?.full_name,
                total_quantity = st.stock_transfer_items.Sum(i => i.quantity),
                status = (int)st.status,
                status_name = st.status.GetDescription(),
                rejection_reason = st.rejection_reason,
                cancel_reason = st.cancel_reason,
                note = st.note,
                created_on = st.created_on,
                created_by = st.created_by,
                modified_on = st.modified_on,
                modified_by = st.modified_by,
                items = st.stock_transfer_items.Select(i => new StockTransferItemView
                {
                    id = i.id,
                    product_id = i.product_id,
                    product_code = i.product?.product_code ?? string.Empty,
                    product_name = i.product?.product_name ?? string.Empty,
                    brand = i.product?.brand,
                    quantity = i.quantity,
                    shipped_quantity = i.shipped_quantity,
                    received_quantity = i.received_quantity,
                    source_warehouse_stock = i.product?.inventories
                        .Where(inv => inv.warehouse_id == st.source_warehouse_id)
                        .Sum(inv => inv.quantity) ?? 0,
                    note = i.note
                }).ToList()
            };
        }

        public async Task<string> CreateAsync(StockTransferCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            if (request.source_warehouse_id == request.destination_warehouse_id)
            {
                throw new BadException(ErrorMessage.SameSourceAndDestinationWarehouse);
            }

            if (request.items == null || !request.items.Any())
            {
                throw new BadException(ErrorMessage.OrderItemsEmpty);
            }

            var srcWh = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == request.source_warehouse_id)
                ?? throw new NotFoundException("Không tìm thấy kho xuất.");
            var destWh = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == request.destination_warehouse_id)
                ?? throw new NotFoundException("Không tìm thấy kho nhận.");

            if (srcWh.status != eStatus_Warehouse.Active || destWh.status != eStatus_Warehouse.Active)
            {
                throw new BadException(ErrorMessage.WarehouseInactive);
            }

            var groupedItems = request.items
                .GroupBy(i => i.product_id)
                .Select(g => new StockTransferItemRequest
                {
                    product_id = g.Key,
                    quantity = g.Sum(x => x.quantity),
                    note = g.Last().note
                })
                .ToList();

            var code = string.IsNullOrWhiteSpace(request.stock_transfer_code)
                ? await GenerateStockTransferCodeAsync()
                : request.stock_transfer_code.Trim().ToUpper();

            var transfer = new stock_transfer
            {
                stock_transfer_code = code,
                source_warehouse_id = request.source_warehouse_id,
                destination_warehouse_id = request.destination_warehouse_id,
                status = eStatus_StockTransfer.Pending,
                transfer_date = request.transfer_date ?? DateTime.Now,
                note = request.note?.Trim()
            }.Created(currentUser.user_id);

            await _context.stock_transfers.AddAsync(transfer);

            foreach (var item in groupedItems)
            {
                await _context.stock_transfer_items.AddAsync(new stock_transfer_item
                {
                    stock_transfer_id = transfer.id,
                    product_id = item.product_id,
                    quantity = item.quantity,
                    shipped_quantity = 0,
                    received_quantity = 0,
                    note = item.note
                }.Created(currentUser.user_id));
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "STOCK_TRANSFER",
                action = "CREATE",
                entity_type = "stock_transfers",
                entity_id = transfer.id,
                description = $"Tạo phiếu chuyển kho '{transfer.stock_transfer_code}' từ {srcWh.warehouse_code} sang {destWh.warehouse_code}.",
                ip_address = ipAddress
            });

            return transfer.id;
        }

        public async Task<bool> ApproveAsync(StockTransferApproveRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var transfer = await _context.stock_transfers
                .Include(st => st.stock_transfer_items)
                    .ThenInclude(i => i.product)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            if (transfer.status != eStatus_StockTransfer.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, transfer.status.GetDescription()));
            }

            // Check source warehouse stock
            foreach (var item in transfer.stock_transfer_items)
            {
                var inv = await _context.inventories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.warehouse_id == transfer.source_warehouse_id && i.product_id == item.product_id);
                var currentQty = inv?.quantity ?? 0;
                if (currentQty < item.quantity)
                {
                    var prodName = item.product?.product_name ?? item.product_id;
                    throw new BadException(string.Format(ErrorMessage.MSG05_InsufficientStock, prodName, currentQty, item.quantity));
                }
            }

            transfer.status = eStatus_StockTransfer.Approved;
            transfer.approved_by = currentUser.user_id;
            transfer.approved_date = DateTime.Now;
            transfer.Modified(currentUser.user_id);

            _context.stock_transfers.Update(transfer);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "STOCK_TRANSFER",
                action = "APPROVE",
                entity_type = "stock_transfers",
                entity_id = transfer.id,
                description = $"Phê duyệt phiếu chuyển kho '{transfer.stock_transfer_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> RejectAsync(StockTransferRejectRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(request.rejection_reason))
            {
                throw new BadException(ErrorMessage.RejectionReasonRequired);
            }

            var transfer = await _context.stock_transfers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            if (transfer.status != eStatus_StockTransfer.Pending)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, transfer.status.GetDescription()));
            }

            transfer.status = eStatus_StockTransfer.Rejected;
            transfer.approved_by = currentUser.user_id;
            transfer.approved_date = DateTime.Now;
            transfer.rejection_reason = request.rejection_reason.Trim();
            transfer.Modified(currentUser.user_id);

            _context.stock_transfers.Update(transfer);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "STOCK_TRANSFER",
                action = "REJECT",
                entity_type = "stock_transfers",
                entity_id = transfer.id,
                description = $"Từ chối phiếu chuyển kho '{transfer.stock_transfer_code}'. Lý do: {transfer.rejection_reason}",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> CancelAsync(StockTransferCancelRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var transfer = await _context.stock_transfers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            if (transfer.status != eStatus_StockTransfer.Pending && transfer.status != eStatus_StockTransfer.Approved)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, transfer.status.GetDescription()));
            }

            transfer.status = eStatus_StockTransfer.Cancelled;
            transfer.cancel_reason = request.cancel_reason?.Trim();
            transfer.Modified(currentUser.user_id);

            _context.stock_transfers.Update(transfer);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "STOCK_TRANSFER",
                action = "CANCEL",
                entity_type = "stock_transfers",
                entity_id = transfer.id,
                description = $"Hủy phiếu chuyển kho '{transfer.stock_transfer_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DispatchTransferAsync(StockTransferDispatchRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var transfer = await _context.stock_transfers
                .Include(st => st.stock_transfer_items)
                    .ThenInclude(i => i.product)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, transfer.source_warehouse_id);

            if (transfer.status != eStatus_StockTransfer.Approved)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, transfer.status.GetDescription()));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in transfer.stock_transfer_items)
                {
                    var inv = await _context.inventories
                        .FirstOrDefaultAsync(i => i.warehouse_id == transfer.source_warehouse_id && i.product_id == item.product_id);

                    var currentQty = inv?.quantity ?? 0;
                    if (inv == null || currentQty < item.quantity)
                    {
                        var prodName = item.product?.product_name ?? item.product_id;
                        throw new BadException(string.Format(ErrorMessage.MSG05_InsufficientStock, prodName, currentQty, item.quantity));
                    }

                    inv.quantity -= item.quantity;
                    inv.last_updated_at = DateTime.Now;
                    inv.Modified(currentUser.user_id);
                    _context.inventories.Update(inv);

                    item.shipped_quantity = item.quantity;
                    item.Modified(currentUser.user_id);
                    _context.stock_transfer_items.Update(item);

                    await _context.inventory_transactions.AddAsync(new inventory_transaction
                    {
                        warehouse_id = transfer.source_warehouse_id,
                        product_id = item.product_id,
                        user_id = currentUser.user_id,
                        stock_transfer_id = transfer.id,
                        transaction_type = eType_InventoryTransaction.TransferOut,
                        quantity = -item.quantity,
                        transaction_date = DateTime.Now,
                        note = request.note ?? $"Xuất chuyển kho theo phiếu {transfer.stock_transfer_code}"
                    }.Created(currentUser.user_id));
                }

                transfer.status = eStatus_StockTransfer.InTransit;
                transfer.dispatched_by = currentUser.user_id;
                transfer.dispatched_date = DateTime.Now;
                transfer.Modified(currentUser.user_id);
                _context.stock_transfers.Update(transfer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _accessLogService.CreateAsync(new AccessLogCreating
                {
                    user_id = currentUser.user_id,
                    module = "STOCK_TRANSFER",
                    action = "DISPATCH",
                    entity_type = "stock_transfers",
                    entity_id = transfer.id,
                    description = $"Xuất chuyển kho phiếu '{transfer.stock_transfer_code}' -> Đang vận chuyển.",
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

        public async Task<bool> ReceiveTransferAsync(StockTransferReceiveRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var transfer = await _context.stock_transfers
                .Include(st => st.stock_transfer_items)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.StockTransferNotFound);

            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, transfer.destination_warehouse_id);

            if (transfer.status != eStatus_StockTransfer.InTransit)
            {
                throw new BadException(string.Format(ErrorMessage.MSG08_InvalidStatusTransition, transfer.status.GetDescription()));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in transfer.stock_transfer_items)
                {
                    var receiveQty = item.shipped_quantity > 0 ? item.shipped_quantity : item.quantity;

                    var inv = await _context.inventories
                        .FirstOrDefaultAsync(i => i.warehouse_id == transfer.destination_warehouse_id && i.product_id == item.product_id);

                    if (inv == null)
                    {
                        inv = new inventory
                        {
                            warehouse_id = transfer.destination_warehouse_id,
                            product_id = item.product_id,
                            quantity = receiveQty,
                            last_updated_at = DateTime.Now
                        }.Created(currentUser.user_id);
                        await _context.inventories.AddAsync(inv);
                    }
                    else
                    {
                        inv.quantity += receiveQty;
                        inv.last_updated_at = DateTime.Now;
                        inv.Modified(currentUser.user_id);
                        _context.inventories.Update(inv);
                    }

                    item.received_quantity = receiveQty;
                    item.Modified(currentUser.user_id);
                    _context.stock_transfer_items.Update(item);

                    await _context.inventory_transactions.AddAsync(new inventory_transaction
                    {
                        warehouse_id = transfer.destination_warehouse_id,
                        product_id = item.product_id,
                        user_id = currentUser.user_id,
                        stock_transfer_id = transfer.id,
                        transaction_type = eType_InventoryTransaction.TransferIn,
                        quantity = receiveQty,
                        transaction_date = DateTime.Now,
                        note = request.note ?? $"Nhận chuyển kho theo phiếu {transfer.stock_transfer_code}"
                    }.Created(currentUser.user_id));
                }

                transfer.status = eStatus_StockTransfer.Completed;
                transfer.received_by = currentUser.user_id;
                transfer.completed_date = DateTime.Now;
                transfer.Modified(currentUser.user_id);
                _context.stock_transfers.Update(transfer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _accessLogService.CreateAsync(new AccessLogCreating
                {
                    user_id = currentUser.user_id,
                    module = "STOCK_TRANSFER",
                    action = "RECEIVE",
                    entity_type = "stock_transfers",
                    entity_id = transfer.id,
                    description = $"Nhận chuyển kho phiếu '{transfer.stock_transfer_code}' tại kho đích -> Hoàn thành.",
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

        private async Task<string> GenerateStockTransferCodeAsync()
        {
            var count = await _context.stock_transfers.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.StockTransfer}-{DateTime.Now:yyyyMM}-{(count + 1):D4}";
        }
    }

    public static class StockTransferServiceExtension
    {
        public static IQueryable<StockTransferTableView> GetQueryable(
            this IQueryable<StockTransferTableView> query,
            StockTransferTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.stock_transfer_code.ToLower().Contains(kw) ||
                    e.source_warehouse_name.ToLower().Contains(kw) ||
                    e.destination_warehouse_name.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.source_warehouse_id))
            {
                query = query.Where(e => e.source_warehouse_id == filter.source_warehouse_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.destination_warehouse_id))
            {
                query = query.Where(e => e.destination_warehouse_id == filter.destination_warehouse_id);
            }

            if (filter.status != null)
            {
                query = query.Where(e => e.status == (int)filter.status.Value);
            }

            if (filter.from_date != null)
            {
                query = query.Where(e => e.transfer_date.Date >= filter.from_date.Value.Date);
            }

            if (filter.to_date != null)
            {
                query = query.Where(e => e.transfer_date.Date <= filter.to_date.Value.Date);
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
