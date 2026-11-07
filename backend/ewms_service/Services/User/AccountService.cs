using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Repository;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.User.Account.Request;
using ewms_service.DTOs.User.Account.Response;
using ewms_service.DTOs.User.Authentication.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.User
{
    public interface IAccountService
    {
        Task<ExtendedPageResponse<AccountTableView>> SearchAsync(AccountTableFilter filter);
        Task<AccountDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(AccountCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(AccountUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateStatusAsync(AccountStatusUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ResetPasswordAsync(AccountResetPasswordRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class AccountService : IAccountService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public AccountService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<AccountTableView>> SearchAsync(AccountTableFilter filter)
        {
            var users = await _context.users
                .AsNoTracking()
                .Include(u => u.user_roles)
                    .ThenInclude(ur => ur.role)
                .Include(u => u.user_warehouses)
                    .ThenInclude(uw => uw.warehouse)
                .OrderByDescending(u => u.created_on)
                .ToListAsync();

            var items = users.Select(u =>
            {
                var primaryRole = u.user_roles.Select(ur => ur.role).FirstOrDefault(r => r != null);
                var warehouses = u.user_warehouses
                    .Where(uw => uw.warehouse != null)
                    .Select(uw => new AssignedWarehouseView
                    {
                        warehouse_id = uw.warehouse!.id,
                        warehouse_code = uw.warehouse.warehouse_code,
                        warehouse_name = uw.warehouse.warehouse_name,
                        address = uw.warehouse.address
                    }).ToList();

                return new AccountTableView
                {
                    id = u.id,
                    employee_code = u.employee_code,
                    full_name = u.full_name,
                    email = u.email,
                    phone = u.phone,
                    status = (int)u.status,
                    status_name = u.status.GetDescription(),
                    role_id = primaryRole?.id ?? string.Empty,
                    role_code = primaryRole?.role_code ?? string.Empty,
                    role_name = primaryRole?.role_name ?? string.Empty,
                    assigned_warehouses = warehouses,
                    created_on = u.created_on,
                    created_by = u.created_by,
                    modified_on = u.modified_on,
                    modified_by = u.modified_by
                };
            }).AsQueryable().GetQueryable(filter);

            return new ExtendedPageResponse<AccountTableView>
            {
                Items = items.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = items.Count()
            };
        }

        public async Task<AccountDetailView> GetByIdAsync(string id)
        {
            var u = await _context.users
                .AsNoTracking()
                .Include(x => x.user_roles)
                    .ThenInclude(ur => ur.role)
                .Include(x => x.user_warehouses)
                    .ThenInclude(uw => uw.warehouse)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            var primaryRole = u.user_roles.Select(ur => ur.role).FirstOrDefault(r => r != null);
            var warehouses = u.user_warehouses
                .Where(uw => uw.warehouse != null)
                .Select(uw => new AssignedWarehouseView
                {
                    warehouse_id = uw.warehouse!.id,
                    warehouse_code = uw.warehouse.warehouse_code,
                    warehouse_name = uw.warehouse.warehouse_name,
                    address = uw.warehouse.address
                }).ToList();

            return new AccountDetailView
            {
                id = u.id,
                employee_code = u.employee_code,
                full_name = u.full_name,
                email = u.email,
                phone = u.phone,
                status = u.status,
                status_name = u.status.GetDescription(),
                role_id = primaryRole?.id ?? string.Empty,
                role_code = primaryRole?.role_code ?? string.Empty,
                role_name = primaryRole?.role_name ?? string.Empty,
                warehouse_ids = warehouses.Select(w => w.warehouse_id).ToList(),
                assigned_warehouses = warehouses,
                created_on = u.created_on,
                modified_on = u.modified_on
            };
        }

        public async Task<string> CreateAsync(AccountCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var empCode = string.IsNullOrWhiteSpace(request.employee_code)
                ? await GenerateEmployeeCodeAsync()
                : request.employee_code.Trim().ToUpper();

            var salt = HashingHelper.GenerateSalt();
            var hash = HashingHelper.HashPassword(request.password, salt);

            var entity = new user
            {
                employee_code = empCode,
                full_name = request.full_name.Trim(),
                email = request.email.Trim().ToLower(),
                phone = request.phone?.Trim(),
                password_salt = salt,
                password_hash = hash,
                status = request.status
            }.Created(currentUser.user_id);

            await _context.users.AddAsync(entity);

            var userRole = new user_role
            {
                user_id = entity.id,
                role_id = request.role_id,
                assigned_by = currentUser.user_id
            }.Created(currentUser.user_id);
            await _context.user_roles.AddAsync(userRole);

            if (request.warehouse_ids != null && request.warehouse_ids.Any())
            {
                foreach (var whId in request.warehouse_ids.Distinct())
                {
                    await _context.user_warehouses.AddAsync(new user_warehouse
                    {
                        user_id = entity.id,
                        warehouse_id = whId
                    }.Created(currentUser.user_id));
                }
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "USER",
                action = "CREATE",
                entity_type = "users",
                entity_id = entity.id,
                description = $"Tạo người dùng '{entity.full_name}' ({entity.employee_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(AccountUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.employee_code))
            {
                entity.employee_code = request.employee_code.Trim().ToUpper();
            }

            entity.full_name = request.full_name.Trim();
            entity.email = request.email.Trim().ToLower();
            entity.phone = request.phone?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.users.Update(entity);

            // Update Role
            var existingRoles = await _context.user_roles.IgnoreQueryFilters().Where(x => x.user_id == entity.id).ToListAsync();
            _context.user_roles.RemoveRange(existingRoles);
            await _context.user_roles.AddAsync(new user_role
            {
                user_id = entity.id,
                role_id = request.role_id,
                assigned_by = currentUser.user_id
            }.Created(currentUser.user_id));

            // Update Assigned Warehouses
            var existingWarehouses = await _context.user_warehouses.IgnoreQueryFilters().Where(x => x.user_id == entity.id).ToListAsync();
            _context.user_warehouses.RemoveRange(existingWarehouses);

            if (request.warehouse_ids != null && request.warehouse_ids.Any())
            {
                foreach (var whId in request.warehouse_ids.Distinct())
                {
                    await _context.user_warehouses.AddAsync(new user_warehouse
                    {
                        user_id = entity.id,
                        warehouse_id = whId
                    }.Created(currentUser.user_id));
                }
            }

            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "USER",
                action = "UPDATE",
                entity_type = "users",
                entity_id = entity.id,
                description = $"Cập nhật người dùng '{entity.full_name}' ({entity.employee_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> UpdateStatusAsync(AccountStatusUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.users.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "USER",
                action = "UPDATE_STATUS",
                entity_type = "users",
                entity_id = entity.id,
                description = $"Đổi trạng thái người dùng '{entity.employee_code}' sang {request.status.GetDescription()}.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> ResetPasswordAsync(AccountResetPasswordRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            var salt = HashingHelper.GenerateSalt();
            entity.password_salt = salt;
            entity.password_hash = HashingHelper.HashPassword(request.new_password, salt);
            entity.Modified(currentUser.user_id);

            _context.users.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "USER",
                action = "RESET_PASSWORD",
                entity_type = "users",
                entity_id = entity.id,
                description = $"Đặt lại mật khẩu cho người dùng '{entity.employee_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            entity.Deleted(currentUser.user_id);
            _context.users.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "USER",
                action = "DELETE",
                entity_type = "users",
                entity_id = entity.id,
                description = $"Xóa người dùng '{entity.employee_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var count = await _context.users.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Employee}{(count + 1):D3}";
        }
    }

    public static class AccountServiceExtension
    {
        public static async Task ValidateAsync(this AccountBase request, ewms_context context, string? excludeId = null)
        {
            if (request is AccountUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.employee_code))
            {
                var code = request.employee_code.Trim().ToUpper();
                var existsCode = await context.users
                    .AsNoTracking()
                    .AnyAsync(x => x.employee_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã nhân viên '{code}'"));
                }
            }

            var email = request.email.Trim().ToLower();
            var existsEmail = await context.users
                .AsNoTracking()
                .AnyAsync(x => x.email.ToLower() == email && (excludeId == null || x.id != excludeId));
            if (existsEmail)
            {
                throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Email '{email}'"));
            }

            var roleExists = await context.roles.AsNoTracking().AnyAsync(r => r.id == request.role_id);
            if (!roleExists)
            {
                throw new NotFoundException(ErrorMessage.RoleNotFound);
            }
        }

        public static IQueryable<AccountTableView> GetQueryable(
            this IQueryable<AccountTableView> query,
            AccountTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.employee_code.ToLower().Contains(kw) ||
                    e.full_name.ToLower().Contains(kw) ||
                    e.email.ToLower().Contains(kw) ||
                    (e.phone != null && e.phone.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.role_id))
            {
                query = query.Where(e => e.role_id == filter.role_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.warehouse_id))
            {
                query = query.Where(e => e.assigned_warehouses.Any(w => w.warehouse_id == filter.warehouse_id));
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
