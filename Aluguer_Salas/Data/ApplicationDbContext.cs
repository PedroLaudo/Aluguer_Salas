// Localização: Aluguer_Salas/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Models; // Garante que todas as tuas classes de modelo estão neste namespace

using Aluguer_Salas.Data;

namespace Aluguer_Salas.Data
{
    // 'Utilizador' é a tua classe que herda de IdentityUser, definida em Aluguer_Salas.Models
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        // DbSets para entidades em Aluguer_Salas.Models
        // É convenção usar o nome da classe no plural para o DbSet
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; } // Nome da propriedade no plural
        public DbSet<Limpeza> Limpezas { get; set; }         // Nome da propriedade no plural
        public DbSet<Utente> Utentes { get; set; }

        // Disponibilidade foi removida

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ESSENCIAL para o Identity funcionar corretamente

            // --- Configuração para Limpeza ---
            modelBuilder.Entity<Limpeza>(entity =>
            {
                // Define a chave primária composta para Limpeza.
                // Garante que Limpeza.cs tem as propriedades IdSala e IdUtilizador.
                entity.HasKey(l => new { l.IdSala, l.IdUtilizador }); // Ajusta se a chave de Limpeza for diferente

                // Relação Limpeza -> Sala (Muitos para 1)
                // Garante que Limpeza.cs tem 'public int IdSala { get; set; }' e 'public virtual Sala Sala { get; set; }'
                // Garante que Sala.cs tem 'public virtual ICollection<Limpeza> Limpezas { get; set; }'
                entity.HasOne(l => l.Sala)
                      .WithMany(s => s.Limpezas) // Assumindo que Sala tem ICollection<Limpeza> Limpezas
                      .HasForeignKey(l => l.IdSala)
                      .OnDelete(DeleteBehavior.Restrict); // Ajusta o comportamento de deleção conforme necessário

                // Relação Limpeza -> Funcionario (Muitos para 1)
                // Garante que Limpeza.cs tem 'public int IdUtilizador { get; set; }' (ou nome da FK para Funcionario)
                // e 'public virtual Funcionario Funcionario { get; set; }'
                // Garante que Funcionario.cs tem 'public virtual ICollection<Limpeza> Limpezas { get; set; }'
                // e uma PK (ex: Id) que corresponde ao tipo de IdUtilizador em Limpeza.
                entity.HasOne(l => l.Funcionario)
                      .WithMany(f => f.Limpezas) // Assumindo que Funcionario tem ICollection<Limpeza> Limpezas
                      .HasForeignKey(l => l.IdUtilizador) // Assumindo que IdUtilizador em Limpeza é a FK para Funcionario.Id
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Configuração para Relação Utilizador (IdentityUser) <-> Utente ---
            modelBuilder.Entity<Utilizador>(entity =>
            {
                // Relação 1 para 1: Utilizador -> Utente
                // Garante que Utilizador.cs tem 'public virtual Utente Utente { get; set; }'
                entity.HasOne(identityUser => identityUser.Utente)
                      .WithOne(utente => utente.Utilizador)
                      // Garante que Utente.cs tem 'public string UtilizadorIdentityId { get; set; }'
                      // e que o tipo corresponde ao Id do IdentityUser (string).
                      .HasForeignKey<Utente>(utente => utente.UtilizadorIdentityId)
                      .OnDelete(DeleteBehavior.Cascade); // Apagar Utente se Utilizador for apagado
            });

            modelBuilder.Entity<Utente>(entity =>
            {
                // Garante que UtilizadorIdentityId é único, pois é uma relação 1 para 1.
                entity.HasIndex(ut => ut.UtilizadorIdentityId)
                      .IsUnique();
            });

            // --- Configuração para Sala ---
            modelBuilder.Entity<Sala>(entity => // Removido o namespace completo, pois 'using Aluguer_Salas.Models;' está no topo
            {
                // Relação Sala -> Reservas (1 para Muitos)
                // Garante que Reserva.cs tem 'public int IdSala { get; set; }' e 'public virtual Sala Sala { get; set; }'
                // Garante que Sala.cs tem 'public virtual ICollection<Reserva> Reservas { get; set; }'
                entity.HasMany(s => s.Reservas)
                      .WithOne(r => r.Sala)
                      .HasForeignKey(r => r.IdSala) // CORRIGIDO para usar a propriedade IdSala da classe Reserva
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Configuração para Reserva ---
            modelBuilder.Entity<Reserva>(entity =>
            {
                // Relação Reserva -> Utilizador (Muitos para 1)
                // Garante que Reserva.cs tem 'public string UtilizadorIdentityId { get; set; }'
                // e 'public virtual Utilizador Utilizador { get; set; }'
                // Opcional: Adiciona 'public virtual ICollection<Reserva> Reservas { get; set; }' em Utilizador.cs
                entity.HasOne(r => r.Utilizador)
                      .WithMany() // Se Utilizador não tem ICollection<Reserva>
                                  // .WithMany(u => u.Reservas) // Se Utilizador TIVER ICollection<Reserva>
                      .HasForeignKey(r => r.UtilizadorIdentityId)
                      .OnDelete(DeleteBehavior.Restrict); // Não apagar utilizador se tiver reservas
            });

            // (Disponibilidade foi removida, então não há configuração para ela aqui)
        }
    }
}