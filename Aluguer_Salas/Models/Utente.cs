// Aluguer_Salas/Data/Utentes.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Data
{
    public class Utente
    {
        [Key] // Chave primária da tabela Utentes
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } = string.Empty; // "Aluno", "Professor", etc.

        [EmailAddress]
        [StringLength(256)] // Tamanho correspondente a AspNetUsers.Email
        public string Email { get; set; } = string.Empty; // Este campo armazenará a cópia do email de AspNetUsers

        // Chave Estrangeira para a tabela Utilizadores (AspNetUsers)
        [Required]
        public string UtilizadorIdentityId { get; set; } = null!;

        // Propriedade de navegação para a entidade Utilizadores (Identity)
        [ForeignKey("UtilizadorIdentityId")]
        public virtual Utilizador Utilizador { get; set; } = null!;
    }
}