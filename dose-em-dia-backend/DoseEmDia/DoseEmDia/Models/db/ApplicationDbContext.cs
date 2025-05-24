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

            // Configuração da tabela Usuario
            modelBuilder.Entity<Usuario>().ToTable("Usuario")
                .HasKey(u => u.IdUser);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.IdUser)
                .HasColumnName("IdUser");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.EnderecoId)
                .HasColumnName("IdEndereco");

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Endereco)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.EnderecoId);

            // Configuração da tabela Vacina
            modelBuilder.Entity<Vacina>().ToTable("Vacina")
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Vacinas)
                .HasForeignKey(v => v.UsuarioId);

            modelBuilder.Entity<Vacina>()
                .Property(v => v.Status)
                .HasConversion<string>();

            // Configuração da tabela Endereco
            modelBuilder.Entity<Endereco>().ToTable("Endereco")
                .Property(e => e.IdEndereco)
                .HasColumnName("IdEndereco");

            modelBuilder.Entity<Endereco>()
                .Property(e => e.CEP)
                .HasColumnName("Cep");

            modelBuilder.Entity<Pais>().ToTable("Paises")
                .HasKey(p => p.IdPais);

            modelBuilder.Entity<Pais>()
                .Property(p => p.IdPais)
                .HasColumnName("IdPais")
                .IsRequired();

            modelBuilder.Entity<ContadorRequisicoes>()
               .Property(e => e.Requisicoes)
               .HasColumnName("Requisicoes");
        }
    }
}
