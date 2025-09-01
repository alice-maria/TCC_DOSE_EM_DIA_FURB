using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models.db;
using DoseEmDia.Helpers;
using DoseEmDia.Services;
using DoseEmDia.Controllers;
using Microsoft.OpenApi.Models;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Services.Interfaces;

namespace DoseEmDia
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Serviços de domínio
            services.AddScoped<EnvioEmail>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<VacinaService>();
            services.AddScoped<NotificacaoService>();
            services.AddScoped<PaisService>();
            services.AddScoped<ComprovanteService>();
            services.AddHostedService<CampanhasEmailService>();
            services.AddHostedService<VacinasEmailService>();
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddScoped<IPostoVacinacaoService, PostoVacinacaoLocService>();

            // --------- Banco de Dados (Railway first) ----------
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // 1) Se houver ConnectionStrings__DefaultConnection, usa (padrão recomendado no Railway)
                var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

                // 2) Senão, tenta o padrão PG* (Railway/Heroku style)
                if (string.IsNullOrWhiteSpace(cs) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PGHOST")))
                {
                    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
                    var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
                    var pgDb = Environment.GetEnvironmentVariable("PGDATABASE");
                    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
                    var pgPwd = Environment.GetEnvironmentVariable("PGPASSWORD");

                    cs = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPwd};" +
                         "SSL Mode=Require;Trust Server Certificate=true";
                }

                // 3) Senão, usa appsettings.json (ambiente local)
                if (string.IsNullOrWhiteSpace(cs))
                {
                    cs = Configuration.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException("Connection string não encontrada.");
                }

                options.UseNpgsql(cs, b =>
                {
                    b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    options.EnableSensitiveDataLogging();
            });

            // --------- Swagger ----------
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dose em Dia API", Version = "v1" });
            });

            // --------- CORS ----------
            services.AddCors(options =>
            {
                options.AddPolicy("Default", builder =>
                {
                    var allowed = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
                    if (!string.IsNullOrWhiteSpace(allowed))
                    {
                        var origins = allowed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        builder.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
                    }
                    else
                    {
                        // Em dev/liberação inicial: libera tudo. Em produção, defina ALLOWED_ORIGINS.
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                });
            });

            // Encaminhamento de cabeçalhos do proxy (Railway)
            services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Se quiser, limpe redes conhecidas para aceitar qualquer proxy:
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Proxy do Railway (gera URLs HTTPS corretas em redirecionamentos/links)
            app.UseForwardedHeaders();

            // NÃO use UseHttpsRedirection no Railway (TLS termina no proxy).
            // app.UseHttpsRedirection();

            // Swagger em Dev sempre; em Prod somente se ENABLE_SWAGGER=true
            var enableSwaggerProd = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
            var swaggerEmProducao = string.Equals(enableSwaggerProd, "true", StringComparison.OrdinalIgnoreCase);

            if (env.IsDevelopment() || swaggerEmProducao)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dose em Dia API v1");
                    if (env.IsDevelopment())
                        c.RoutePrefix = string.Empty; // raiz em dev
                });
            }

            app.UseRouting();
            app.UseCors("Default");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Health-check simples
                endpoints.MapGet("/health", () => Results.Ok(new { status = "ok" }));
            });

            // Migração automática + seed
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var applied = db.Database.GetAppliedMigrations().ToList();
            var pending = db.Database.GetPendingMigrations().ToList();
            Console.WriteLine($"Applied: {applied.Count} => {string.Join(",", applied)}");
            Console.WriteLine($"Pending: {pending.Count} => {string.Join(",", pending)}");

            db.Database.Migrate(); // aplica migrations

            var paisService = scope.ServiceProvider.GetRequiredService<PaisService>();
            paisService.PopularPaisesSeNecessarioAsync().GetAwaiter().GetResult();
        }
    }
}