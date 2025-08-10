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
                var pgHost = Environment.GetEnvironmentVariable("PGHOST");

                string cs;
                if (!string.IsNullOrWhiteSpace(pgHost))
                {
                    var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
                    var pgDb = Environment.GetEnvironmentVariable("PGDATABASE");
                    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
                    var pgPwd = Environment.GetEnvironmentVariable("PGPASSWORD");

                    cs = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPwd};SSL Mode=Require;Trust Server Certificate=true";
                }
                else
                {
                    // Ambiente local: usa appsettings.json
                    cs = Configuration.GetConnectionString("DefaultConnection");
                }

                options.UseNpgsql(cs, b =>
                    b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    options.EnableSensitiveDataLogging();
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

                var applied = db.Database.GetAppliedMigrations().ToList();
                var pending = db.Database.GetPendingMigrations().ToList();
                Console.WriteLine($"Applied: {applied.Count} => {string.Join(",", applied)}");
                Console.WriteLine($"Pending: {pending.Count} => {string.Join(",", pending)}");

                db.Database.Migrate(); // <-- aplica antes do seed

                var paisService = scope.ServiceProvider.GetRequiredService<PaisService>();
                paisService.PopularPaisesSeNecessarioAsync().GetAwaiter().GetResult();
            }
        }
    }
}
