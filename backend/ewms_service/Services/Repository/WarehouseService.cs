using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Repository;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Repository.Warehouse.Request;
using ewms_service.DTOs.Repository.Warehouse.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Repository
{
    public interface IWarehouseService
    {
        Task<ExtendedPageResponse<WarehouseTableView>> SearchAsync(WarehouseTableFilter filter);
        Task<WarehouseDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(WarehouseCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(WarehouseUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class WarehouseService : IWarehouseService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public WarehouseService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<WarehouseTableView>> SearchAsync(WarehouseTableFilter filter)
        {
            var warehouses = await _context.warehouses
                .AsNoTracking()
                .Include(w => w.inventories)
                .Include(w => w.user_warehouses)
                .OrderBy(w => w.warehouse_code)
                .Select(w => new WarehouseTableView
                {
                    id = w.id,
                    warehouse_code = w.warehouse_code,
                    warehouse_name = w.warehouse_name,
                    address = w.address,
                    status = (int)w.status,
                    status_name = w.status.GetDescription(),
                    total_products = w.inventories.Count(i => i.quantity > 0),
                    total_quantity = w.inventories.Sum(i => i.quantity),
                    assigned_staff_count = w.user_warehouses.Count,
                    created_on = w.created_on,
                    created_by = w.created_by,
                    modified_on = w.modified_on,
                    modified_by = w.modified_by
                })
                .ToListAsync();

            var query = warehouses.AsQueryable().GetQueryable(filter);

            return new ExtendedPageResponse<WarehouseTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<WarehouseDetailView> GetByIdAsync(string id)
        {
            var w = await _context.warehouses
                .AsNoTracking()
                .Include(x => x.inventories)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.WarehouseNotFound);

            return new WarehouseDetailView
            {
                id = w.id,
                warehouse_code = w.warehouse_code,
                warehouse_name = w.warehouse_name,
                address = w.address,
                status = w.status,
                status_name = w.status.GetDescription(),
                total_products = w.inventories.Count(i => i.quantity > 0),
                total_quantity = w.inventories.Sum(i => i.quantity),
                created_on = w.created_on,
                modified_on = w.modified_on
            };
        }

        public async Task<string> CreateAsync(WarehouseCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.warehouse_code)
                ? await GenerateWarehouseCodeAsync()
                : request.warehouse_code.Trim().ToUpper();

            var entity = new warehouse
            {
                warehouse_code = code,
                warehouse_name = request.warehouse_name.Trim(),
                address = request.address?.Trim(),
                status = request.status
            }.Created(currentUser.user_id);

            await _context.warehouses.AddAsync(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "WAREHOUSE",
                action = "CREATE",
                entity_type = "warehouses",
                entity_id = entity.id,
                description = $"Tạo kho hàng '{entity.warehouse_name}' ({entity.warehouse_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(WarehouseUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.warehouses.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.WarehouseNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.warehouse_code))
            {
                entity.warehouse_code = request.warehouse_code.Trim().ToUpper();
            }

            entity.warehouse_name = request.warehouse_name.Trim();
            entity.address = request.address?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.warehouses.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "WAREHOUSE",
                action = "UPDATE",
                entity_type = "warehouses",
                entity_id = entity.id,
                description = $"Cập nhật kho hàng '{entity.warehouse_name}' ({entity.warehouse_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.warehouses.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.WarehouseNotFound);

            entity.Deleted(currentUser.user_id);
            _context.warehouses.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "WAREHOUSE",
                action = "DELETE",
                entity_type = "warehouses",
                entity_id = entity.id,
                description = $"Xóa kho hàng '{entity.warehouse_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateWarehouseCodeAsync()
        {
            var count = await _context.warehouses.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Warehouse}-{(count + 1):D2}";
        }
    }

    public static class WarehouseServiceExtension
    {
        public static async Task ValidateAsync(this WarehouseBase request, ewms_context context, string? excludeId = null)
        {
            if (request is WarehouseUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.warehouse_code))
            {
                var code = request.warehouse_code.Trim().ToUpper();
                var existsCode = await context.warehouses
                    .AsNoTracking()
                    .AnyAsync(x => x.warehouse_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã kho '{code}'"));
                }
            }

            var name = request.warehouse_name.Trim().ToLower();
            var existsName = await context.warehouses
                .AsNoTracking()
                .AnyAsync(x => x.warehouse_name.ToLower() == name && (excludeId == null || x.id != excludeId));
            if (existsName)
            {
                throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Tên kho '{request.warehouse_name}'"));
            }
        }

        public static IQueryable<WarehouseTableView> GetQueryable(
            this IQueryable<WarehouseTableView> query,
            WarehouseTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.warehouse_code.ToLower().Contains(kw) ||
                    e.warehouse_name.ToLower().Contains(kw) ||
                    (e.address != null && e.address.ToLower().Contains(kw)));
            }

            if (filter.status != null)
            {
                query = query.Where(e => e.status == (int)filter.status.Value);
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
