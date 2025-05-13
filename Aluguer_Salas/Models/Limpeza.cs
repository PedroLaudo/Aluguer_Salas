using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Data;

namespace Aluguer_Salas.Models // Assumindo que todos os modelos est�o aqui
{
    public class Limpeza
    {
        [Key] // Chave Prim�ria da entidade Limpeza
        public int LimpezaId { get; set; }

        [Required(ErrorMessage = "A sala � obrigat�ria.")]
        public int IdSala { get; set; } // Chave Estrangeira para Sala

        [Required(ErrorMessage = "O funcion�rio � obrigat�rio.")]
        public int FuncionarioId { get; set; } // Chave Estrangeira para Funcionario

        [Required(ErrorMessage = "O dia da semana � obrigat�rio.")]
        [StringLength(50, ErrorMessage = "O dia da semana deve ter no m�ximo 50 caracteres.")]
        public string DiaSemana { get; set; } = string.Empty;

        // Propriedades de Navega��o
        [ForeignKey("IdSala")]
        public virtual Sala Sala { get; set; } = null!; // Uma limpeza DEVE ter uma sala

        [ForeignKey("FuncionarioId")]
        public virtual Funcionario Funcionario { get; set; } = null!; // Uma limpeza DEVE ter um funcion�rio
    }
}