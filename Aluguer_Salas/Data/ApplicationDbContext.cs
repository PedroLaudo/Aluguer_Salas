using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Models; // Namespace principal dos seus modelos

namespace Aluguer_Salas.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        // DbSets para suas entidades
        public DbSet<Sala> Salas { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<Funcionario> Funcionarios { get; set; } = null!; // Pluralizado
        public DbSet<Limpeza> Limpezas { get; set; } = null!;       // Pluralizado
        public DbSet<Utente> Utentes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relação Utilizador (Identity) <-> Utente (Um-para-Um ou Um-para-Zero-ou-Um)
            modelBuilder.Entity<Utilizador>(entity =>
            {
                entity.HasOne(u => u.Utente)        // Utilizador tem um Utente (propriedade de navegação em Utilizador.cs)
                      .WithOne(ut => ut.Utilizador) // Utente tem um Utilizador (propriedade de navegação em Utente.cs)
                      .HasForeignKey<Utente>(ut => ut.UtilizadorIdentityId); // Chave estrangeira está em Utente e se chama UtilizadorId
                                                                             // (Certifique-se que Utente.cs tem 'public string UtilizadorId { get; set; }')
            });

            modelBuilder.Entity<Utente>(entity =>
            {
                // Garante que um Utilizador só pode estar associado a um Utente (opcional, mas bom para 1-para-1)
                entity.HasIndex(ut => ut.UtilizadorIdentityId)
                      .IsUnique();
            });

            // Relação Utilizador (Identity) <-> Funcionario (Um Utilizador pode ser um Funcionario)
            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.HasOne(f => f.Utilizador) // Funcionario tem um Utilizador (propriedade de navegação em Funcionario.cs)
                      .WithMany()               // IdentityUser (Utilizador) não terá uma coleção explícita de Funcionarios aqui.
                                                // Se um Utilizador pudesse ser muitos Funcionarios (não faz sentido neste contexto), você adicionaria .WithMany(u => u.Funcionarios) e a coleção em Utilizador.cs
                      .HasForeignKey(f => f.UtilizadorId) // Chave estrangeira está em Funcionario e se chama UtilizadorId
                      .IsRequired(); // Um Funcionario DEVE ter um Utilizador associado

                // Se você quiser garantir que um Utilizador só pode ser UM Funcionario:
                // entity.HasIndex(f => f.UtilizadorId).IsUnique();
            });

            // Relação Sala <-> Reserva (Um-para-Muitos)
            modelBuilder.Entity<Sala>(entity =>
            {
                entity.HasMany(s => s.Reservas) // Sala tem muitas Reservas (propriedade de navegação ICollection<Reserva> em Sala.cs)
                      .WithOne(r => r.Sala)     // Reserva pertence a uma Sala (propriedade de navegação Sala em Reserva.cs)
                      .HasForeignKey(r => r.IdSala) // Chave estrangeira está em Reserva e se chama IdSala (CORRIGIDO)
                      .OnDelete(DeleteBehavior.Restrict); // O que acontece se uma Sala for apagada
            });
            // A relação Reserva <-> Utilizador (IdentityUser) é implicitamente tratada se você tem
            // 'public string UtilizadorIdentityId { get; set; }' e 'public virtual Utilizador Utilizador { get; set; }' em Reserva.cs

            // Relação Sala <-> Limpeza (Um-para-Muitos)
            modelBuilder.Entity<Sala>(entity =>
            {
                entity.HasMany(s => s.Limpezas) // Sala tem muitas Limpezas (ICollection<Limpeza> em Sala.cs)
                      .WithOne(l => l.Sala)     // Limpeza pertence a uma Sala (Sala em Limpeza.cs)
                      .HasForeignKey(l => l.IdSala) // Chave estrangeira está em Limpeza e se chama IdSala
                      .OnDelete(DeleteBehavior.Cascade); // Ex: Se apagar Sala, apaga Limpezas associadas
            });

            // Relação Funcionario <-> Limpeza (Um-para-Muitos)
            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.HasMany(f => f.Limpezas) // Funcionario tem muitas Limpezas (ICollection<Limpeza> em Funcionario.cs)
                      .WithOne(l => l.Funcionario) // Limpeza pertence a um Funcionario (Funcionario em Limpeza.cs)
                      .HasForeignKey(l => l.FuncionarioId) // Chave estrangeira está em Limpeza e se chama FuncionarioId (CORRIGIDO)
                      .OnDelete(DeleteBehavior.Restrict); // O que acontece se um Funcionario for apagado
            });

            // Configuração da entidade Limpeza (PK já definida por [Key] em Limpeza.cs)
            // Se Limpeza NÃO tivesse [Key] e você quisesse chave composta, seria aqui:
            // modelBuilder.Entity<Limpeza>(entity =>
            // {
            //     entity.HasKey(l => new { l.IdSala, l.FuncionarioId, l.DiaSemana });
            // });
        }
    }
}