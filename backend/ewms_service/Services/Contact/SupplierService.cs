using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Contact;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Contact.Supplier.Request;
using ewms_service.DTOs.Contact.Supplier.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Contact
{
    public interface ISupplierService
    {
        Task<ExtendedPageResponse<SupplierTableView>> SearchAsync(SupplierTableFilter filter);
        Task<SupplierDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(SupplierCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(SupplierUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class SupplierService : ISupplierService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public SupplierService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<SupplierTableView>> SearchAsync(SupplierTableFilter filter)
        {
            var list = await _context.suppliers
                .AsNoTracking()
                .Include(s => s.purchase_orders)
                .OrderByDescending(s => s.created_on)
                .Select(s => new SupplierTableView
                {
                    id = s.id,
                    supplier_code = s.supplier_code,
                    supplier_name = s.supplier_name,
                    contact_person = s.contact_person,
                    phone = s.phone,
                    email = s.email,
                    address = s.address,
                    status = (int)s.status,
                    status_name = s.status.GetDescription(),
                    total_purchase_orders = s.purchase_orders.Count,
                    created_on = s.created_on,
                    created_by = s.created_by,
                    modified_on = s.modified_on,
                    modified_by = s.modified_by
                })
                .ToListAsync();

            var query = list.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<SupplierTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<SupplierDetailView> GetByIdAsync(string id)
        {
            var s = await _context.suppliers
                .AsNoTracking()
                .Include(x => x.purchase_orders)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.SupplierNotFound);

            return new SupplierDetailView
            {
                id = s.id,
                supplier_code = s.supplier_code,
                supplier_name = s.supplier_name,
                contact_person = s.contact_person,
                phone = s.phone,
                email = s.email,
                address = s.address,
                status = s.status,
                status_name = s.status.GetDescription(),
                total_purchase_orders = s.purchase_orders.Count,
                created_on = s.created_on,
                modified_on = s.modified_on
            };
        }

        public async Task<string> CreateAsync(SupplierCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.supplier_code)
                ? await GenerateSupplierCodeAsync()
                : request.supplier_code.Trim().ToUpper();

            var entity = new supplier
            {
                supplier_code = code,
                supplier_name = request.supplier_name.Trim(),
                contact_person = request.contact_person?.Trim(),
                phone = request.phone?.Trim(),
                email = request.email?.Trim(),
                address = request.address?.Trim(),
                status = request.status
            }.Created(currentUser.user_id);

            await _context.suppliers.AddAsync(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "SUPPLIER",
                action = "CREATE",
                entity_type = "suppliers",
                entity_id = entity.id,
                description = $"Tạo nhà cung cấp '{entity.supplier_name}' ({entity.supplier_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(SupplierUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.suppliers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.SupplierNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.supplier_code))
            {
                entity.supplier_code = request.supplier_code.Trim().ToUpper();
            }

            entity.supplier_name = request.supplier_name.Trim();
            entity.contact_person = request.contact_person?.Trim();
            entity.phone = request.phone?.Trim();
            entity.email = request.email?.Trim();
            entity.address = request.address?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.suppliers.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "SUPPLIER",
                action = "UPDATE",
                entity_type = "suppliers",
                entity_id = entity.id,
                description = $"Cập nhật nhà cung cấp '{entity.supplier_name}' ({entity.supplier_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.suppliers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.SupplierNotFound);

            entity.Deleted(currentUser.user_id);
            _context.suppliers.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "SUPPLIER",
                action = "DELETE",
                entity_type = "suppliers",
                entity_id = entity.id,
                description = $"Xóa nhà cung cấp '{entity.supplier_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateSupplierCodeAsync()
        {
            var count = await _context.suppliers.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Supplier}-{(count + 1):D4}";
        }
    }

    public static class SupplierServiceExtension
    {
        public static async Task ValidateAsync(this SupplierBase request, ewms_context context, string? excludeId = null)
        {
            if (request is SupplierUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.supplier_code))
            {
                var code = request.supplier_code.Trim().ToUpper();
                var existsCode = await context.suppliers
                    .AsNoTracking()
                    .AnyAsync(x => x.supplier_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã nhà cung cấp '{code}'"));
                }
            }
        }

        public static IQueryable<SupplierTableView> GetQueryable(
            this IQueryable<SupplierTableView> query,
            SupplierTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.supplier_code.ToLower().Contains(kw) ||
                    e.supplier_name.ToLower().Contains(kw) ||
                    (e.contact_person != null && e.contact_person.ToLower().Contains(kw)) ||
                    (e.phone != null && e.phone.ToLower().Contains(kw)) ||
                    (e.email != null && e.email.ToLower().Contains(kw)));
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
