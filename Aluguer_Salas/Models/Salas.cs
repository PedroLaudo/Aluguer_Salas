using System.ComponentModel.DataAnnotations;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Data
{
    public class Salas
    {
        [Key]
        public int Id { get; set; }

        // Validação para garantir que o nome da sala é obrigatório e não exceda 255 caracteres
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        public string NomeSala { get; set; }

        // Validação para garantir que a capacidade da sala é obrigatória
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        public int Capacidade { get; set; }

        // A descrição da sala pode ser opcional
        public string Descricao { get; set; }

        // Relacionamento com a tabela Disponibilidade (se houver)
      //  public ICollection<Disponibilidade> Disponibilidades { get; set; }

        // Relacionamento com a tabela Reserva (se houver)
        public ICollection<Reservas> Reservas { get; set; }

        // lista limpezas



    }
}
