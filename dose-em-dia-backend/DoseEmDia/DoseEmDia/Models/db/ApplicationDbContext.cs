using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1:1 entre Usuario e Endereco
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Endereco)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.EnderecoId);

            // 1:N entre Usuario e Vacinas
            modelBuilder.Entity<Vacina>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Vacinas)
                .HasForeignKey(v => v.UsuarioId);

            // Enum como string
            modelBuilder.Entity<Vacina>()
                .Property(v => v.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Usuario>().ToTable("Usuario");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Id)
                .HasColumnName("IdUser");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Senha)
                .HasColumnName("Senha");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.EnderecoId)
                .HasColumnName("IdEndereco");

            modelBuilder.Entity<Endereco>().ToTable("Endereco");

            modelBuilder.Entity<Endereco>()
                .Property(e => e.Id)
                .HasColumnName("IdEndereco");

            modelBuilder.Entity<Endereco>()
                .Property(e => e.CEP)
                .HasColumnName("Cep");

        }

    }
}
