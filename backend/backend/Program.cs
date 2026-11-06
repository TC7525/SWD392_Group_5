using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Repositories;
using backend.Services;

namespace backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Services
            builder.Services.AddScoped<IPOStatusService, POStatusService>();
            builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
