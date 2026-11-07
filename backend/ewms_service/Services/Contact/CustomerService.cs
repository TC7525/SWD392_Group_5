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
using ewms_service.DTOs.Contact.Customer.Request;
using ewms_service.DTOs.Contact.Customer.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Contact
{
    public interface ICustomerService
    {
        Task<ExtendedPageResponse<CustomerTableView>> SearchAsync(CustomerTableFilter filter);
        Task<CustomerDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(CustomerCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(CustomerUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public CustomerService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<CustomerTableView>> SearchAsync(CustomerTableFilter filter)
        {
            var list = await _context.customers
                .AsNoTracking()
                .Include(c => c.outbound_orders)
                .OrderByDescending(c => c.created_on)
                .Select(c => new CustomerTableView
                {
                    id = c.id,
                    customer_code = c.customer_code,
                    customer_name = c.customer_name,
                    phone = c.phone,
                    email = c.email,
                    address = c.address,
                    status = (int)c.status,
                    status_name = c.status.GetDescription(),
                    total_orders = c.outbound_orders.Count,
                    created_on = c.created_on,
                    created_by = c.created_by,
                    modified_on = c.modified_on,
                    modified_by = c.modified_by
                })
                .ToListAsync();

            var query = list.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<CustomerTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<CustomerDetailView> GetByIdAsync(string id)
        {
            var c = await _context.customers
                .AsNoTracking()
                .Include(x => x.outbound_orders)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.CustomerNotFound);

            return new CustomerDetailView
            {
                id = c.id,
                customer_code = c.customer_code,
                customer_name = c.customer_name,
                phone = c.phone,
                email = c.email,
                address = c.address,
                status = c.status,
                status_name = c.status.GetDescription(),
                total_orders = c.outbound_orders.Count,
                created_on = c.created_on,
                modified_on = c.modified_on
            };
        }

        public async Task<string> CreateAsync(CustomerCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.customer_code)
                ? await GenerateCustomerCodeAsync()
                : request.customer_code.Trim().ToUpper();

            var entity = new customer
            {
                customer_code = code,
                customer_name = request.customer_name.Trim(),
                phone = request.phone?.Trim(),
                email = request.email?.Trim(),
                address = request.address?.Trim(),
                status = request.status
            }.Created(currentUser.user_id);

            await _context.customers.AddAsync(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CUSTOMER",
                action = "CREATE",
                entity_type = "customers",
                entity_id = entity.id,
                description = $"Tạo khách hàng '{entity.customer_name}' ({entity.customer_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(CustomerUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.customers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.CustomerNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.customer_code))
            {
                entity.customer_code = request.customer_code.Trim().ToUpper();
            }

            entity.customer_name = request.customer_name.Trim();
            entity.phone = request.phone?.Trim();
            entity.email = request.email?.Trim();
            entity.address = request.address?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.customers.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CUSTOMER",
                action = "UPDATE",
                entity_type = "customers",
                entity_id = entity.id,
                description = $"Cập nhật khách hàng '{entity.customer_name}' ({entity.customer_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.customers.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.CustomerNotFound);

            entity.Deleted(currentUser.user_id);
            _context.customers.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CUSTOMER",
                action = "DELETE",
                entity_type = "customers",
                entity_id = entity.id,
                description = $"Xóa khách hàng '{entity.customer_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateCustomerCodeAsync()
        {
            var count = await _context.customers.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Customer}-{(count + 1):D4}";
        }
    }

    public static class CustomerServiceExtension
    {
        public static async Task ValidateAsync(this CustomerBase request, ewms_context context, string? excludeId = null)
        {
            if (request is CustomerUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.customer_code))
            {
                var code = request.customer_code.Trim().ToUpper();
                var existsCode = await context.customers
                    .AsNoTracking()
                    .AnyAsync(x => x.customer_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã khách hàng '{code}'"));
                }
            }
        }

        public static IQueryable<CustomerTableView> GetQueryable(
            this IQueryable<CustomerTableView> query,
            CustomerTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.customer_code.ToLower().Contains(kw) ||
                    e.customer_name.ToLower().Contains(kw) ||
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
