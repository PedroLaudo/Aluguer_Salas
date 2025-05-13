using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Data;

namespace Aluguer_Salas.Models // Assumindo que todos os modelos estão aqui
{
    public class Limpeza
    {
        [Key] // Chave Primária da entidade Limpeza
        public int LimpezaId { get; set; }

        [Required(ErrorMessage = "A sala é obrigatória.")]
        public int IdSala { get; set; } // Chave Estrangeira para Sala

        [Required(ErrorMessage = "O funcionário é obrigatório.")]
        public int FuncionarioId { get; set; } // Chave Estrangeira para Funcionario

        [Required(ErrorMessage = "O dia da semana é obrigatório.")]
        [StringLength(50, ErrorMessage = "O dia da semana deve ter no máximo 50 caracteres.")]
        public string DiaSemana { get; set; } = string.Empty;

        // Propriedades de Navegação
        [ForeignKey("IdSala")]
        public virtual Sala Sala { get; set; } = null!; // Uma limpeza DEVE ter uma sala

        [ForeignKey("FuncionarioId")]
        public virtual Funcionario Funcionario { get; set; } = null!; // Uma limpeza DEVE ter um funcionário
    }
}