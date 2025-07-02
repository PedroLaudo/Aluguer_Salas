using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// Se a classe Limpeza estiver no namespace Aluguer_Salas.Data, adicione este using:
using Aluguer_Salas.Data;
// using Aluguer_Salas.Models; // Se Limpeza estiver aqui, o using acima não é necessário

namespace Aluguer_Salas.Models
{
    public class Sala
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        [Display(Name = "Nome da Sala")]
        public string NomeSala { get; set; } = string.Empty;

        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser pelo menos 1.")]
        public int Capacidade { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Descricao { get; set; }

        public bool Disponivel { get; set; } = true;

       
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        
        
    }
}