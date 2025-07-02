using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Define as tabelas na base de dados
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Utente> Utentes { get; set; }
        public DbSet<Material> Materiais { get; set; }
        public DbSet<RequisicaoMaterial> RequisicoesMaterial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relação 1: Um Utilizador tem um Utente (1:1)
            // O Utente tem uma FK (UtilizadorIdentityId) que referencia o Utilizador 
            // Se o Utilizador for apagado, o Utente correspondente também será apagado (Cascade)
            modelBuilder.Entity<Utilizador>(entity =>
            {
                entity.HasOne(identityUser => identityUser.Utente)
                      .WithOne(utente => utente.Utilizador)
                      .HasForeignKey<Utente>(utente => utente.UtilizadorIdentityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Garante que a chave estrangeira UtilizadorIdentityId seja única na tabela Utentes
            // Isto reforça a relação 1:1 entre Utente e Utilizador
            modelBuilder.Entity<Utente>(entity =>
            {
                entity.HasIndex(ut => ut.UtilizadorIdentityId)
                      .IsUnique();
            });

            // Relação 2: Uma Sala pode ter muitas Reservas (1:N)
            // Uma Reserva pertence a uma única Sala
            // Ao apagar uma Sala, não apaga as Reservas (Restrict)
            modelBuilder.Entity<Sala>(entity =>
            {
                entity.HasMany(s => s.Reservas)
                      .WithOne(r => r.Sala)
                      .HasForeignKey(r => r.IdSala)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Relação 3: Uma Reserva é feita por um Utilizador
            // Um Utilizador pode ter várias Reservas 
            // Ao apagar o Utilizador, não apaga as Reservas (Restrict)
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasOne(r => r.Utilizador)
                      .WithMany()
                      .HasForeignKey(r => r.UtilizadorIdentityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    
    }
}
