using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Category;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Category.Request;
using ewms_service.DTOs.Category.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Category
{
    public interface ICategoryService
    {
        Task<ExtendedPageResponse<CategoryTableView>> SearchAsync(CategoryTableFilter filter);
        Task<CategoryDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(CategoryCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(CategoryUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public CategoryService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<CategoryTableView>> SearchAsync(CategoryTableFilter filter)
        {
            var list = await _context.categories
                .AsNoTracking()
                .Include(c => c.products)
                .OrderByDescending(c => c.created_on)
                .Select(c => new CategoryTableView
                {
                    id = c.id,
                    category_code = c.category_code,
                    category_name = c.category_name,
                    description = c.description,
                    status = (int)c.status,
                    status_name = c.status.GetDescription(),
                    product_count = c.products.Count,
                    created_on = c.created_on,
                    created_by = c.created_by,
                    modified_on = c.modified_on,
                    modified_by = c.modified_by
                })
                .ToListAsync();

            var query = list.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<CategoryTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }

        public async Task<CategoryDetailView> GetByIdAsync(string id)
        {
            var c = await _context.categories
                .AsNoTracking()
                .Include(x => x.products)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.CategoryNotFound);

            return new CategoryDetailView
            {
                id = c.id,
                category_code = c.category_code,
                category_name = c.category_name,
                description = c.description,
                status = c.status,
                status_name = c.status.GetDescription(),
                product_count = c.products.Count,
                created_on = c.created_on,
                modified_on = c.modified_on
            };
        }

        public async Task<string> CreateAsync(CategoryCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.category_code)
                ? await GenerateCategoryCodeAsync()
                : request.category_code.Trim().ToUpper();

            var entity = new category
            {
                category_code = code,
                category_name = request.category_name.Trim(),
                description = request.description?.Trim(),
                status = request.status
            }.Created(currentUser.user_id);

            await _context.categories.AddAsync(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CATEGORY",
                action = "CREATE",
                entity_type = "categories",
                entity_id = entity.id,
                description = $"Tạo danh mục '{entity.category_name}' ({entity.category_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(CategoryUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.categories.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.CategoryNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.category_code))
            {
                entity.category_code = request.category_code.Trim().ToUpper();
            }

            entity.category_name = request.category_name.Trim();
            entity.description = request.description?.Trim();
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.categories.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CATEGORY",
                action = "UPDATE",
                entity_type = "categories",
                entity_id = entity.id,
                description = $"Cập nhật danh mục '{entity.category_name}' ({entity.category_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.categories
                .Include(x => x.products)
                .FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.CategoryNotFound);

            if (entity.products.Any())
            {
                throw new BadException(ErrorMessage.CategoryHasProducts);
            }

            entity.Deleted(currentUser.user_id);
            _context.categories.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "CATEGORY",
                action = "DELETE",
                entity_type = "categories",
                entity_id = entity.id,
                description = $"Xóa danh mục '{entity.category_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateCategoryCodeAsync()
        {
            var count = await _context.categories.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Category}-{(count + 1):D3}";
        }
    }

    public static class CategoryServiceExtension
    {
        public static async Task ValidateAsync(this CategoryBase request, ewms_context context, string? excludeId = null)
        {
            if (request is CategoryUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.category_code))
            {
                var code = request.category_code.Trim().ToUpper();
                var existsCode = await context.categories
                    .AsNoTracking()
                    .AnyAsync(x => x.category_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã danh mục '{code}'"));
                }
            }

            var name = request.category_name.Trim().ToLower();
            var existsName = await context.categories
                .AsNoTracking()
                .AnyAsync(x => x.category_name.ToLower() == name && (excludeId == null || x.id != excludeId));
            if (existsName)
            {
                throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Tên danh mục '{request.category_name}'"));
            }
        }

        public static IQueryable<CategoryTableView> GetQueryable(
            this IQueryable<CategoryTableView> query,
            CategoryTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.category_code.ToLower().Contains(kw) ||
                    e.category_name.ToLower().Contains(kw) ||
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
