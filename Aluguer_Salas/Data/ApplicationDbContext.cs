using Microsoft.EntityFrameworkCore;
// using Aluguer_Salas.Data; // Desnecessário se já está no namespace Aluguer_Salas.Data
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
        public DbSet<Utentes> Utentes { get; set; } = null!; // Inicializar para evitar aviso CS8618

        // public DbSet<Utilizadores> Utilizadores { get; set; } // REDUNDANTE
        // IdentityDbContext<Utilizadores> já expõe uma propriedade `DbSet<Utilizadores> Users`.
        // Se precisar acessá-los como `context.Utilizadores`, pode criar um alias ou usar `context.Users`.
        // Removi para evitar confusão. Use `this.Users` para aceder aos Utilizadores do Identity.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Limpeza>()
                .HasKey(l => new { l.IdSala, l.IdUtilizador });

            // Configuração da relação 1-para-1 entre Utilizadores e Utentes
            modelBuilder.Entity<Utilizadores>()
                .HasOne(identityUser => identityUser.Utente) // Propriedade de navegação em Utilizadores
                .WithOne(utente => utente.Utilizador)    // Propriedade de navegação em Utentes
                .HasForeignKey<Utentes>(utente => utente.UtilizadorIdentityId); // FK em Utentes
        }
    }
}