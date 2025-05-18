using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aluguer_Salas.Models
{
    public class RequisicaoMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [ForeignKey("UtilizadorId")]
        public virtual Utilizador Utilizador { get; set; } = null!;

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataRequisicao { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFim { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        public int QuantidadeRequisitada { get; set; }
    }
}
