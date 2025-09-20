using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata;
using DoseEmDia.Models.Localizacao;

namespace DoseEmDia.Models.db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; } = default!;
        public DbSet<Vacina> Vacina { get; set; } = default!;
        public DbSet<Notificacao> Notificacao { get; set; } = default!;
        public DbSet<Pais> Pais { get; set; } = default!;
        public DbSet<Estado> Estado { get; set; } = default!;
        public DbSet<Cidade> Cidade { get; set; } = default!;
        public DbSet<Cep> Cep { get; set; } = default!;
        public DbSet<Endereco> Endereco { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var toUnspecified = new ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

            var toUnspecifiedNullable = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(toUnspecified);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(toUnspecifiedNullable);
                }
            }

            // Usuario
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUser);
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Endereco)
                .WithMany()
                .HasForeignKey(u => u.EnderecoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.DataNascimento)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.TokenExpiracao)
                .HasColumnType("timestamp without time zone");

            // Vacina
            modelBuilder.Entity<Vacina>().ToTable("Vacina");
            modelBuilder.Entity<Vacina>().HasKey(v => v.IdVacina);
            modelBuilder.Entity<Vacina>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Vacinas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vacina>()
                .Property(v => v.DataAplicacao)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Vacina>()
                .Property(v => v.Status)
                .HasConversion<string>(); 

            // Notificacoes
            modelBuilder.Entity<Notificacao>().ToTable("Notificacoes");
            modelBuilder.Entity<Notificacao>().HasKey(n => n.IdNotificacao);
            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.Usuario)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notificacao>()
                .Property(n => n.DataEnvio)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Notificacao>()
                .Property(n => n.Tipo)
                .HasConversion<string>();

            // Pais
            modelBuilder.Entity<Pais>(e =>
            {
                e.ToTable("Pais");                
                e.HasKey(x => x.IdPais);
                e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
                e.Property(x => x.Url).HasMaxLength(300);
                e.HasIndex(x => x.Nome).IsUnique();
            });

            // Estado
            modelBuilder.Entity<Estado>(e =>
            {
                e.ToTable("Estado");
                e.HasKey(x => x.IdEstado);
                e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
                e.Property(x => x.Uf).IsRequired().HasMaxLength(2);
                e.HasOne(x => x.Pais)
                    .WithMany(p => p.Estados)
                    .HasForeignKey(x => x.PaisId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.PaisId, x.Nome }).IsUnique();
            });

            // Cidade
            modelBuilder.Entity<Cidade>(e =>
            {
                e.ToTable("Cidade");
                e.HasKey(x => x.IdCidade);
                e.Property(x => x.Nome).IsRequired().HasMaxLength(160);
                e.HasOne(x => x.Estado)
                    .WithMany(s => s.Cidades)
                    .HasForeignKey(x => x.EstadoId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.EstadoId, x.Nome }).IsUnique();
            });

            //CEP
            modelBuilder.Entity<Cep>(e =>
            {
                e.ToTable("Cep");
                e.HasKey(x => x.IdCep);
                e.Property(x => x.Codigo).IsRequired().HasMaxLength(9);
                e.Property(x => x.Bairro).HasMaxLength(160);

                e.HasOne(x => x.Cidade)
                    .WithMany(c => c.Ceps)
                    .HasForeignKey(x => x.CidadeId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.HasIndex(x => x.CidadeId);
            });

            // Endereço
            modelBuilder.Entity<Endereco>(e =>
            {
                e.ToTable("Endereco");
                e.HasKey(x => x.IdEndereco);

                e.Property(x => x.Logradouro).HasMaxLength(255);
                e.Property(x => x.Numero).IsRequired().HasMaxLength(20);  

                e.HasOne(x => x.Cep)
                    .WithMany(c => c.Enderecos)
                    .HasForeignKey(x => x.CepId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.CepId);
            });

        }
    }
}
