using ewms_service.Services.AccessLog;
using ewms_service.Services.Category;
using ewms_service.Services.Contact;
using ewms_service.Services.Product;
using ewms_service.Services.Repository;
using ewms_service.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace ewms_service
{
    public static class ServiceInjection
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpContextAccessor();

            // Phase 2: Auth, User, Role, Warehouse, AccessLog
            services.AddScoped<IAccessLogService, AccessLogService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IWarehouseService, WarehouseService>();

            // Phase 3: Master Data (Category, Product, Customer, Supplier)
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();

            // Phase 4, 5 & 6: Purchase Order, Outbound Order, Stock Transfer, Inventory & Dashboard
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IOutboundOrderService, OutboundOrderService>();
            services.AddScoped<IStockTransferService, StockTransferService>();
            services.AddScoped<IInventoryService, InventoryService>();
        }
    }
}
