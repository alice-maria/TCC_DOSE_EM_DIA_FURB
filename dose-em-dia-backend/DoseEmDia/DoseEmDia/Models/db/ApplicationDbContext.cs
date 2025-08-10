using DoseEmDia.Controllers;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Models.db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Endereco> Endereco { get; set; }
        public DbSet<Vacina> Vacina { get; set; }
        public DbSet<Pais> Paises { get; set; }
        public DbSet<Notificacao> Notificacao { get; set; }
        public DbSet<ContadorRequisicoes> ContadorRequisicoes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Endereco
            modelBuilder.Entity<Endereco>().ToTable("Endereco");
            modelBuilder.Entity<Endereco>().HasKey(e => e.IdEndereco);

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
                .HasConversion<string>(); // isso converte enum para texto

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

            // Paises
            modelBuilder.Entity<Pais>().ToTable("Paises");
            modelBuilder.Entity<Pais>().HasKey(p => p.IdPais);

            // Contador de Requisições
            modelBuilder.Entity<ContadorRequisicoes>().ToTable("ContadorRequisicoes");
            modelBuilder.Entity<ContadorRequisicoes>().HasKey(c => c.Id);
        }
    }
}
