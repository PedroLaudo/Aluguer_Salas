
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
namespace Aluguer_Salas.Models 
{
    public class Sala
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "O {0} � de preenchimento obrigat�rio.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no m�ximo {1} caracteres.")]
        [Display(Name = "Nome da Sala")]
        public string NomeSala { get; set; } = string.Empty;

        [Required(ErrorMessage = "A {0} � de preenchimento obrigat�rio.")]
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser pelo menos 1.")]
        public int Capacidade { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Descricao { get; set; }

        public bool Disponivel { get; set; } = true;

        // Rela��es com inicializa��o e virtual
        // Assume que Disponibilidade est� em Aluguer_Salas.Data
        public virtual ICollection<Disponibilidade> Disponibilidades { get; set; } = new List<Disponibilidade>();

        // Assume que Reservas est� em Aluguer_Salas.Data
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}