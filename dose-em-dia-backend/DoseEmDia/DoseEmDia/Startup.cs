using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models.db;
using DoseEmDia.Helpers;
using DoseEmDia.Services;
using DoseEmDia.Controllers;
using Microsoft.OpenApi.Models;

namespace DoseEmDia
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddScoped<EnvioEmail>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<VacinaService>();
            services.AddScoped<NotificacaoService>();
            services.AddScoped<PaisService>();
            services.AddScoped<ComprovanteService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var cs = Configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(cs))
                {
                    var host = Environment.GetEnvironmentVariable("PGHOST");
                    var port = Environment.GetEnvironmentVariable("PGPORT");
                    var db = Environment.GetEnvironmentVariable("PGDATABASE");
                    var user = Environment.GetEnvironmentVariable("PGUSER");
                    var pwd = Environment.GetEnvironmentVariable("PGPASSWORD");

                    cs = $"Host={host};Port={port};Database={db};Username={user};Password={pwd};SSL Mode=Require;Trust Server Certificate=true";
                }

                options.UseNpgsql(cs);

                // ATENÇÃO: só ligue em DEV
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dose em Dia API",
                    Version = "v1"
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Default", builder =>
                {
                    builder
                        .AllowAnyOrigin()   // em produção: substitua por .WithOrigins("https://seu-front.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Ambiente de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dose em Dia API v1");
                    c.RoutePrefix = string.Empty; // Swagger na raiz: https://localhost:5001/
                });
            }

            app.UseRouting();

            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.MigrateAsync();

                var paisService = scope.ServiceProvider.GetRequiredService<PaisService>();
                paisService.PopularPaisesSeNecessarioAsync().GetAwaiter().GetResult();
            }
        }
    }
}
