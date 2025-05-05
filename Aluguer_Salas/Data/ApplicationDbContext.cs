using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Aluguer_Salas.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizadores>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Reservas> Reservas { get; set; }
        public DbSet<Salas> Salas { get; set; }
        public DbSet<Disponibilidade> Disponibilidades { get; set; }
        public DbSet<Funcionario> Funcionario { get; set; }
        public DbSet<Limpeza> Limpeza { get; set; }
        public DbSet<Utentes> Utentes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Definir chave primária composta para a entidade Limpeza
            modelBuilder.Entity<Limpeza>()
                .HasKey(l => new { l.IdSala, l.IdUtilizador }); // Chave composta

            // Configurações adicionais se necessário
        }
    }
}
