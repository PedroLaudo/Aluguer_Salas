using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Aluguer_Salas.Models
{
    public class RequisicaoMaterial
    {
        [Key]
        public int Id { get; set; }

        public string UtilizadorId { get; set; } = string.Empty;

        [ForeignKey("UtilizadorId")]
        [ValidateNever]
        public virtual Utilizador? Utilizador { get; set; }

        [Required]
        [Display(Name = "Material")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        [ValidateNever]
        public virtual Material? Material { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Requisição")]
        public DateTime DataRequisicao { get; set; }

        [Required]
        [Display(Name = "Hora Início")]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Display(Name = "Hora fim")]
        public TimeSpan HoraFim { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        [Display(Name = "Quantidade")]
        public int QuantidadeRequisitada { get; set; }
    }
}
