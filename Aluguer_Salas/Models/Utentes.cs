using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Data
{
    public class Utentes
    {
        /// <summary>
        /// Identificador do utilizador
        /// </summary>
        [Key] 
        public int IdUtilizador { get; set; }

        /// <summary>
        /// Email do utilizador
        /// </summary>
        [StringLength(60, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        public string Email { get; set; }

        /// <summary>
        /// Tipo do utilizador (professor, aluno, pessoa externa)
        /// </summary>
        public string Tipo { get; set; }

        // Relacionamento com a tabela Utilizadores
        public Utilizadores Utilizador { get; set; }
    }
}
