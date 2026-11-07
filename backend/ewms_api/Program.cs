using System.Text;
using ewms_common;
using ewms_common.Authentications;
using ewms_common.Configurations;
using ewms_infra.Context;
using ewms_service;
using ewms_service.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

AppSettings.Instance.SetConfiguration(builder.Configuration);

var startup = new BaseStartUp(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=ewms_db;Username=postgres;Password=postgres";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ewms_context>(options =>
{
    options.UseNpgsql(dataSource, opt =>
    {
        opt.CommandTimeout(10000);
        opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

var jwtSecret = builder.Configuration.GetValue<string>("JwtConfig:Secret") ?? "EWMS_Super_Secret_Key_2026_SWD392_Group5_32Bytes!";
var jwtIssuer = builder.Configuration.GetValue<string>("JwtConfig:ValidIssuer") ?? "ewms_api";
var jwtAudience = builder.Configuration.GetValue<string>("JwtConfig:ValidAudience") ?? "ewms_client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddPolicyScheme(DynamicAuth.AuthenticationScheme, DynamicAuth.AuthenticationScheme, options =>
{
    options.ForwardDefaultSelector = DynamicAuth.OnForwardDefault;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
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

builder.Services.AddServices();
startup.ConfigurationService(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ewms_context>();
        await DbInitializer.SeedAsync(dbContext);
        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not migrate or seed database on startup. Please verify PostgreSQL connection settings in appsettings.json.");
    }
}

app.Run();
