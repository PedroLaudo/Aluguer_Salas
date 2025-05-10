// Localização: Aluguer_Salas/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Models; // MANTIDO: Necessário para Salas
namespace Aluguer_Salas.Data
{
    // Assume que Utilizadores, Reservas, Disponibilidade, Funcionario, Limpeza, Utentes estão em Aluguer_Salas.Data
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Disponibilidade> Disponibilidades { get; set; }
        public DbSet<Funcionario> Funcionario { get; set; }
        public DbSet<Limpeza> Limpeza { get; set; }
        public DbSet<Utente> Utentes { get; set; } = default!; // Ou null! se preferir

        // DbSet para entidade em Aluguer_Salas.Models
        public DbSet<Aluguer_Salas.Models.Sala> Salas { get; set; } // MANTIDO: Correto

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Importante para Identity

            // Configuração para Limpeza
            modelBuilder.Entity<Limpeza>(entity =>
            {
                // VERIFIQUE: A classe Limpeza.cs TEM as propriedades IdSala e IdUtilizador?
                entity.HasKey(l => new { l.IdSala, l.IdUtilizador });
            });

            // Configuração para Relação Utilizadores <-> Utentes
            modelBuilder.Entity<Utilizador>(entity =>
            {
                // VERIFIQUE: A classe Utilizadores.cs TEM a propriedade de navegação 'public virtual Utentes Utente { get; set; }'?
                entity.HasOne(identityUser => identityUser.Utente)
                      .WithOne(utente => utente.Utilizador)
                      // VERIFIQUE: A classe Utentes.cs TEM a propriedade de chave estrangeira 'public string UtilizadorIdentityId { get; set; }'? (O tipo deve corresponder ao Id do IdentityUser, geralmente string)
                      .HasForeignKey<Utente>(utente => utente.UtilizadorIdentityId);
            });

            modelBuilder.Entity<Utente>(entity =>
            {
                // VERIFIQUE: A classe Utentes.cs TEM a propriedade de chave estrangeira 'public string UtilizadorIdentityId { get; set; }'?
                entity.HasIndex(ut => ut.UtilizadorIdentityId)
                      .IsUnique();
                // VERIFIQUE: A classe Utentes.cs TEM a propriedade de navegação 'public virtual Utilizadores Utilizador { get; set; }'? (Usada implicitamente pelo WithOne anterior)
            });

            // ADICIONAR: Configuração explícita das relações de Salas (Boa Prática)
            // Ajuda o EF Core e pode expor problemas mais cedo.
            modelBuilder.Entity<Aluguer_Salas.Models.Sala>(entity =>
            {
                // Relação Salas -> Reservas (1 para Muitos)
                // Assume que Reservas.cs tem 'public int SalaId { get; set; }' e 'public virtual Salas Sala { get; set; }'
                entity.HasMany(s => s.Reservas)
                      .WithOne(r => r.Sala)
                      .HasForeignKey("SalaId") // Especifique o nome da FK em Reservas se não seguir a convenção (ex: IdSala)
                      .OnDelete(DeleteBehavior.Restrict); // Ou Cascade, SetNull, etc., dependendo da sua regra de negócio

                // Relação Salas -> Disponibilidades (1 para Muitos)
                // Assume que Disponibilidade.cs tem 'public int IdSala { get; set; }' e 'public virtual Salas Sala { get; set; }'
                entity.HasMany(s => s.Disponibilidades)
                      .WithOne(d => d.Sala)
                      .HasForeignKey(d => d.IdSala) // Nome da FK em Disponibilidade
                      .OnDelete(DeleteBehavior.Cascade); // Exemplo: Apagar disponibilidades se a sala for apagada
            });

            // Relação Salas -> Limpeza (1 para Muitos)
            // Assume que Limpeza.cs tem 'public int IdSala { get; set; }' e 'public virtual Salas Sala { get; set; }'
            modelBuilder.Entity<Limpeza>() // Configurar a partir de Limpeza
                 .HasOne(l => l.Sala)
                 .WithMany() // Se Salas não tiver uma ICollection<Limpeza>
                             // .WithMany(s => s.Limpezas) // Se Salas TIVER uma ICollection<Limpeza>
                 .HasForeignKey(l => l.IdSala)
                 .OnDelete(DeleteBehavior.Restrict); // Exemplo

            // Relação Funcionario -> Limpeza (1 para Muitos)
            // Assume que Limpeza.cs tem 'public int IdUtilizador { get; set; }' (como FK para Funcionario) e 'public virtual Funcionario Funcionario { get; set; }'
            // Assume que Funcionario.cs tem 'public int Id { get; set; }' (PK) e talvez 'public virtual ICollection<Limpeza> Limpezas { get; set; }'
            modelBuilder.Entity<Limpeza>()
                .HasOne(l => l.Funcionario)
                .WithMany() // Se Funcionario não tiver uma ICollection<Limpeza>
                            // .WithMany(f => f.Limpezas) // Se Funcionario TIVER uma ICollection<Limpeza>
                .HasForeignKey(l => l.IdUtilizador) // Assumindo que IdUtilizador em Limpeza é a FK para Funcionario.Id
                .OnDelete(DeleteBehavior.Restrict); // Exemplo
        }
    }
}