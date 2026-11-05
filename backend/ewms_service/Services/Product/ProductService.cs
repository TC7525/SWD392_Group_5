using ewms_common.Authentications;
using ewms_common.DTOs.Request;
using ewms_common.Exceptions;
using ewms_common.Extensions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.Product;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.Product.Request;
using ewms_service.DTOs.Product.Response;
using ewms_service.Services.AccessLog;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.Product
{
    public interface IProductService
    {
        Task<ExtendedPageResponse<ProductTableView>> SearchAsync(ProductTableFilter filter, UserHeader currentUser);
        Task<ProductDetailView> GetByIdAsync(string id);
        Task<string> CreateAsync(ProductCreating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> UpdateAsync(ProductUpdating request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null);
    }

    public class ProductService : IProductService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public ProductService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<ExtendedPageResponse<ProductTableView>> SearchAsync(ProductTableFilter filter, UserHeader currentUser)
        {
            var targetWarehouseId = !string.IsNullOrWhiteSpace(filter.warehouse_id)
                ? filter.warehouse_id
                : currentUser.warehouse_id;

            var products = await _context.products
                .AsNoTracking()
                .Include(p => p.category)
                .Include(p => p.inventories)
                .OrderByDescending(p => p.created_on)
                .ToListAsync();

            var items = products.Select(p =>
            {
                var currentWhStock = !string.IsNullOrWhiteSpace(targetWarehouseId)
                    ? p.inventories.Where(i => i.warehouse_id == targetWarehouseId).Sum(i => i.quantity)
                    : p.inventories.Sum(i => i.quantity);
                var totalStock = p.inventories.Sum(i => i.quantity);
                var isLowStock = currentWhStock <= p.minimum_stock_level;

                return new ProductTableView
                {
                    id = p.id,
                    product_code = p.product_code,
                    product_name = p.product_name,
                    category_id = p.category_id,
                    category_name = p.category?.category_name ?? string.Empty,
                    brand = p.brand,
                    description = p.description,
                    unit_price = p.unit_price,
                    minimum_stock_level = p.minimum_stock_level,
                    current_warehouse_stock = currentWhStock,
                    total_stock = totalStock,
                    is_low_stock = isLowStock,
                    status = (int)p.status,
                    status_name = p.status.GetDescription(),
                    created_on = p.created_on,
                    created_by = p.created_by,
                    modified_on = p.modified_on,
                    modified_by = p.modified_by
                };
            }).AsQueryable().GetQueryable(filter);

            return new ExtendedPageResponse<ProductTableView>
            {
                Items = items.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = items.Count()
            };
        }

        public async Task<ProductDetailView> GetByIdAsync(string id)
        {
            var p = await _context.products
                .AsNoTracking()
                .Include(x => x.category)
                .Include(x => x.inventories)
                    .ThenInclude(i => i.warehouse)
                .FirstOrDefaultAsync(x => x.id == id)
                ?? throw new NotFoundException(ErrorMessage.ProductNotFound);

            var warehouseStocks = p.inventories
                .Where(i => i.warehouse != null)
                .Select(i => new ProductWarehouseStockView
                {
                    warehouse_id = i.warehouse_id,
                    warehouse_code = i.warehouse!.warehouse_code,
                    warehouse_name = i.warehouse.warehouse_name,
                    quantity = i.quantity,
                    is_low_stock = i.quantity <= p.minimum_stock_level,
                    last_updated_at = i.last_updated_at
                })
                .OrderBy(w => w.warehouse_code)
                .ToList();

            return new ProductDetailView
            {
                id = p.id,
                product_code = p.product_code,
                product_name = p.product_name,
                category_id = p.category_id,
                category_name = p.category?.category_name ?? string.Empty,
                brand = p.brand,
                description = p.description,
                unit_price = p.unit_price,
                minimum_stock_level = p.minimum_stock_level,
                status = p.status,
                status_name = p.status.GetDescription(),
                total_stock = p.inventories.Sum(i => i.quantity),
                warehouse_stocks = warehouseStocks,
                created_on = p.created_on,
                modified_on = p.modified_on
            };
        }

        public async Task<string> CreateAsync(ProductCreating request, UserHeader currentUser, string? ipAddress = null)
        {
            await request.ValidateAsync(_context);

            var code = string.IsNullOrWhiteSpace(request.product_code)
                ? await GenerateProductCodeAsync()
                : request.product_code.Trim().ToUpper();

            var entity = new product
            {
                product_code = code,
                product_name = request.product_name.Trim(),
                category_id = request.category_id,
                brand = request.brand?.Trim(),
                description = request.description?.Trim(),
                unit_price = request.unit_price,
                minimum_stock_level = request.minimum_stock_level,
                status = request.status
            }.Created(currentUser.user_id);

            await _context.products.AddAsync(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PRODUCT",
                action = "CREATE",
                entity_type = "products",
                entity_id = entity.id,
                description = $"Tạo sản phẩm '{entity.product_name}' ({entity.product_code}).",
                ip_address = ipAddress
            });

            return entity.id;
        }

        public async Task<bool> UpdateAsync(ProductUpdating request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.products.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.ProductNotFound);

            await request.ValidateAsync(_context);

            if (!string.IsNullOrWhiteSpace(request.product_code))
            {
                entity.product_code = request.product_code.Trim().ToUpper();
            }

            entity.product_name = request.product_name.Trim();
            entity.category_id = request.category_id;
            entity.brand = request.brand?.Trim();
            entity.description = request.description?.Trim();
            entity.unit_price = request.unit_price;
            entity.minimum_stock_level = request.minimum_stock_level;
            entity.status = request.status;
            entity.Modified(currentUser.user_id);

            _context.products.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PRODUCT",
                action = "UPDATE",
                entity_type = "products",
                entity_id = entity.id,
                description = $"Cập nhật sản phẩm '{entity.product_name}' ({entity.product_code}).",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<bool> DeleteAsync(DeleteRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var entity = await _context.products.FirstOrDefaultAsync(x => x.id == request.id)
                ?? throw new NotFoundException(ErrorMessage.ProductNotFound);

            entity.Deleted(currentUser.user_id);
            _context.products.Update(entity);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "PRODUCT",
                action = "DELETE",
                entity_type = "products",
                entity_id = entity.id,
                description = $"Xóa sản phẩm '{entity.product_code}'.",
                ip_address = ipAddress
            });

            return true;
        }

        private async Task<string> GenerateProductCodeAsync()
        {
            var count = await _context.products.IgnoreQueryFilters().CountAsync();
            return $"{CodePrefix.Product}-{(count + 1):D4}";
        }
    }

    public static class ProductServiceExtension
    {
        public static async Task ValidateAsync(this ProductBase request, ewms_context context, string? excludeId = null)
        {
            if (request is ProductUpdating updating)
            {
                excludeId = updating.id;
            }

            if (!string.IsNullOrWhiteSpace(request.product_code))
            {
                var code = request.product_code.Trim().ToUpper();
                var existsCode = await context.products
                    .AsNoTracking()
                    .AnyAsync(x => x.product_code.ToUpper() == code && (excludeId == null || x.id != excludeId));
                if (existsCode)
                {
                    throw new BadException(string.Format(ErrorMessage.MSG04_DuplicateCodeOrEmail, $"Mã sản phẩm '{code}'"));
                }
            }

            var catExists = await context.categories.AsNoTracking().AnyAsync(c => c.id == request.category_id);
            if (!catExists)
            {
                throw new NotFoundException(ErrorMessage.CategoryNotFound);
            }
        }

        public static IQueryable<ProductTableView> GetQueryable(
            this IQueryable<ProductTableView> query,
            ProductTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.product_code.ToLower().Contains(kw) ||
                    e.product_name.ToLower().Contains(kw) ||
                    (e.brand != null && e.brand.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.category_id))
            {
                query = query.Where(e => e.category_id == filter.category_id);
            }

            if (filter.low_stock_only == true)
            {
                query = query.Where(e => e.is_low_stock);
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
