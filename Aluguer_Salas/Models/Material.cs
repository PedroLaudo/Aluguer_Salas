using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do material é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do material não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Quantidade Disponível")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade disponível deve ser pelo menos 1.")]
        public int QuantidadeDisponivel { get; set; }

        // Relação com as requisições
        public virtual ICollection<RequisicaoMaterial> Requisicoes { get; set; } = new List<RequisicaoMaterial>();
    }
}
