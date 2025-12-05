using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models.db;
using DoseEmDia.Helpers;
using DoseEmDia.Services;
using DoseEmDia.Controllers;
using Microsoft.OpenApi.Models;
using SendGrid;

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

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddSingleton<ISendGridClient>(sp =>
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                             ?? Configuration["SendGrid:ApiKey"]
                             ?? throw new InvalidOperationException("SendGrid API key não configurada.");
                return new SendGridClient(apiKey);
            });
            services.AddScoped<EnvioEmail>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<VacinaService>();
            services.AddScoped<NotificacaoService>();
            services.AddScoped<PaisService>();
            services.AddScoped<ComprovanteService>();
            services.AddHostedService<CampanhasEmailService>();
            services.AddScoped<VacinasEmailService>();
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHttpClient<DoseEmDia.Services.Geo.PostoVacinacaoLocService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

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

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dose em Dia API", Version = "v1" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Default", builder =>
                {
                    builder
                        .AllowAnyOrigin()    
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            var enableSwaggerProd = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
            var swaggerEmProducao = string.Equals(enableSwaggerProd, "true", StringComparison.OrdinalIgnoreCase);

            if (env.IsDevelopment() || swaggerEmProducao)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dose em Dia API v1");
                    if (env.IsDevelopment())
                        c.RoutePrefix = string.Empty;
                });
            }

            app.UseRouting();
            app.UseCors("Default");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/health", () => Results.Ok(new { status = "ok" }));
            });

            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var applied = db.Database.GetAppliedMigrations().ToList();
            var pending = db.Database.GetPendingMigrations().ToList();
            Console.WriteLine($"Applied: {applied.Count} => {string.Join(",", applied)}");
            Console.WriteLine($"Pending: {pending.Count} => {string.Join(",", pending)}");

            db.Database.Migrate();

            var paisService = scope.ServiceProvider.GetRequiredService<PaisService>();
            paisService.PopularPaisesSeNecessarioAsync().GetAwaiter().GetResult();
        }
    }
}