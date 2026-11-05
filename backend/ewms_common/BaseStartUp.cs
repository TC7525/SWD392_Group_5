using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using ewms_common.Configurations;
using System.Text.Json.Serialization;

namespace ewms_common
{
    public class BaseStartUp
    {
        public IConfiguration Configuration { get; set; }

        public BaseStartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigurationService(IServiceCollection services)
        {
            services.AddControllers(options =>
                    {
                        options.ModelBinderProviders.Insert(0, new LocalDateTimeModelBinderProvider());
                    })
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                        options.JsonSerializerOptions.Converters.Add(new LocalDateTimeConverter());
                        options.JsonSerializerOptions.WriteIndented = true;
                    });
            services.AddHealthChecks();
            services.AddHttpClient();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EWMS API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddCors(o => o.AddPolicy("AllowOrigin", builder =>
            {
                builder
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
            }));

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 2147483648;
            });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatFormatHandling = FloatFormatHandling.Symbol,
                FloatParseHandling = FloatParseHandling.Decimal,
            };

            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Action<IApplicationBuilder>? beforeAuthorization = null)
        {
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowOrigin");
            app.UseRouting();
            app.UseAuthentication();

            beforeAuthorization?.Invoke(app);

            app.UseAuthorization();
            app.UseHealthChecks("/health");
            ((IEndpointRouteBuilder)app).MapControllers();
        }
    }
}
