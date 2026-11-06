using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using backend.Common;
using backend.Data;
using backend.Models;
using backend.Repositories;
using backend.Services;

namespace backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Controllers & Global Exception Filter
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            // 2. DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3. CORS policy AllowReactApp
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // 4. JWT Authentication
            var jwtSecret = builder.Configuration["Jwt:SecretKey"]
                ?? "EWMN_Secret_Key_For_Demo_Authentication_Token_2026_Minimum_32_Characters!";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EWMN";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EWMN";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddAuthorization();

            // 5. Repositories
            builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // 6. Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPOStatusService, POStatusService>();
            builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IReceivingService, ReceivingService>();

            // 7. Swagger / OpenAPI Documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "EWMS - Electronic Warehouse Management System API",
                    Version = "v1",
                    Description = "API tài liệu và thử nghiệm cho các use cases Quản lý Mua hàng & Nhập kho (UC09 – UC14)"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "Nhập token JWT theo định dạng: Bearer {token}",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // 7. Initialize & Seed Database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    if (context.Database.IsRelational())
                    {
                        await context.Database.EnsureCreatedAsync();
                    }
                    await DbInitializer.InitializeAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetService<ILogger<Program>>();
                    logger?.LogWarning(ex, "Database initialization/seeding could not complete on startup: {Message}", ex.Message);
                }
            }

            // 8. Configure HTTP Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            // Enable Swagger and Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EWMS API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseRouting();

            app.UseCors("AllowReactApp");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
