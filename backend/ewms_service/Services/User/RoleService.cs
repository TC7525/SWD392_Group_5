using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Permission;
using ewms_service.Consts;
using ewms_service.DTOs.User.Role.Request;
using ewms_service.DTOs.User.Role.Response;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.User
{
    public interface IRoleService
    {
        Task<ExtendedPageResponse<RoleTableView>> SearchAsync(RoleTableFilter filter);
        Task<RoleDetailView> GetByIdAsync(string id);
        Task<List<PermissionView>> GetAllPermissionsAsync();
        Task<string> CreateAsync(RoleCreating request, UserHeader currentUser);
        Task<bool> UpdateAsync(RoleUpdating request, UserHeader currentUser);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser);
    }

    public class RoleService : IRoleService
    {
        private readonly ewms_context _context;

        public RoleService(ewms_context context)
        {
            _context = context;
        }

        public async Task<ExtendedPageResponse<RoleTableView>> SearchAsync(RoleTableFilter filter)
        {
            var roles = await _context.roles
                .AsNoTracking()
                .Include(r => r.user_roles)
                .Include(r => r.role_permissions)
                    .ThenInclude(rp => rp.permission)
                .OrderBy(r => r.role_code)
                .ToListAsync();

            var items = roles.Select(r => new RoleTableView
            {
                id = r.id,
                role_code = r.role_code,
                role_name = r.role_name,
                description = r.description,
                status = (int)r.status,
                status_name = r.status.GetDescription(),
                user_count = r.user_roles.Count,
                permissions = r.role_permissions
                    .Where(rp => rp.permission != null)
                    .Select(rp => new PermissionView
                    {
                        id = rp.permission!.id,
                        permission_code = rp.permission.permission_code,
                        permission_name = rp.permission.permission_name,
                        module = rp.permission.module,
                        action = rp.permission.action,
                        description = rp.permission.description
                    }).ToList(),
                created_on = r.created_on,
                created_by = r.created_by,
                modified_on = r.modified_on,
                modified_by = r.modified_by
            }).AsQueryable().GetQueryable(filter);

            return new ExtendedPageResponse<RoleTableView>
            {
                Items = items.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = items.Count()
            };
        }

        public async Task<RoleDetailView> GetByIdAsync(string id)
        {
            var r = await _context.roles
                .AsNoTracking()
                .Include(x => x.role_permissions)
                    .ThenInclude(rp => rp.permission)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.RoleNotFound);

            var perms = r.role_permissions
                .Where(rp => rp.permission != null)
                .Select(rp => new PermissionView
                {
                    id = rp.permission!.id,
                    permission_code = rp.permission.permission_code,
                    permission_name = rp.permission.permission_name,
                    module = rp.permission.module,
                    action = rp.permission.action,
                    description = rp.permission.description
                }).ToList();

            return new RoleDetailView
            {
                id = r.id,
                role_code = r.role_code,
                role_name = r.role_name,
                description = r.description,
                status = r.status,
                status_name = r.status.GetDescription(),
                permission_ids = perms.Select(p => p.id).ToList(),
                permissions = perms
            };
        }

        public async Task<List<PermissionView>> GetAllPermissionsAsync()
        {
            return await _context.permissions
                .AsNoTracking()
                .OrderBy(p => p.module)
                .ThenBy(p => p.permission_code)
                .Select(p => new PermissionView
                {
                    id = p.id,
                    permission_code = p.permission_code,
                    permission_name = p.permission_name,
                    module = p.module,
                    action = p.action,
                    description = p.description
                })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(RoleCreating request, UserHeader currentUser)
        {
            await request.ValidateAsync(_context);

            var entity = new role
            {
                role_code = request.role_code.Trim().ToUpper(),
                role_name = request.role_name.Trim(),
                description = request.description?.Trim(),
                status = request.status
            }.Created(currentUser.user_id);

            await _context.roles.AddAsync(entity);

            if (request.permission_ids != null && request.permission_ids.Any())
            {
                foreach (var permId in request.permission_ids.Distinct())
                {
                    await _context.role_permissions.AddAsync(new role_permission
                    {
                        role_id = entity.id,
                        permission_id = permId
                    }.Created(currentUser.user_id));
                }
            }

            await _context.SaveChangesAsync();
            return entity.id;
        }

        public async Task<bool> UpdateAsync(RoleUpdating request, UserHeader currentUser)
        {
            var entity = await _context.roles.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.RoleNotFound);

            await request.ValidateAsync(_context);

            entity.role_code = request.role_code.Trim().ToUpper();
            entity.role_name = request.role_name.Trim();
            entity.description = request.description?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.roles.Update(entity);

            var existingPerms = await _context.role_permissions.IgnoreQueryFilters().Where(x => x.role_id == entity.id).ToListAsync();
            _context.role_permissions.RemoveRange(existingPerms);

            if (request.permission_ids != null && request.permission_ids.Any())
            {
                foreach (var permId in request.permission_ids.Distinct())
                {
                    await _context.role_permissions.AddAsync(new role_permission
                    {
                        role_id = entity.id,
                        permission_id = permId
                    }.Created(currentUser.user_id));
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser)
        {
            var entity = await _context.roles.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.RoleNotFound);

            entity.Deleted(currentUser.user_id);
            _context.roles.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public static class RoleServiceExtension
    {
        public static async Task ValidateAsync(this RoleBase request, ewms_context context, string? excludeId = null)
        {
            if (request is RoleUpdating updating)
            {
                excludeId = updating.id;
            }

            var code = request.role_code.Trim().ToUpper();
            var existsCode = await context.roles
                .AsNoTracking()
                .AnyAsync(x => x.role_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
            if (existsCode)
            {
                throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã vai trò '{code}'"));
            }
        }

        public static IQueryable<RoleTableView> GetQueryable(
            this IQueryable<RoleTableView> query,
            RoleTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.role_code.ToLower().Contains(kw) ||
                    e.role_name.ToLower().Contains(kw) ||
                    (e.description != null && e.description.ToLower().Contains(kw)));
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
