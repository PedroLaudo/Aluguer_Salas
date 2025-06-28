using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Sala> Salas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Utente> Utentes { get; set; }
        public DbSet<Material> Materiais { get; set; }
        public DbSet<RequisicaoMaterial> RequisicoesMaterial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Utilizador>(entity =>
            {
                entity.HasOne(identityUser => identityUser.Utente)
                      .WithOne(utente => utente.Utilizador)
                      .HasForeignKey<Utente>(utente => utente.UtilizadorIdentityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Utente>(entity =>
            {
                entity.HasIndex(ut => ut.UtilizadorIdentityId)
                      .IsUnique();
            });

            modelBuilder.Entity<Sala>(entity =>
            {
                entity.HasMany(s => s.Reservas)
                      .WithOne(r => r.Sala)
                      .HasForeignKey(r => r.IdSala)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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
