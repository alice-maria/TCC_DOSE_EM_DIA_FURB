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

        // DbSet representa as tabelas no banco de dados
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Vacina> Vacinas { get; set; }

        // Mapear o enum como string (opcional)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento entre Usuário e Endereço
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Endereco)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.EnderecoId);

            // Mapear o enum StatusVacina como string
            modelBuilder.Entity<Vacina>()
                .Property(v => v.Status)
                .HasConversion<string>();
        }

    }
}
