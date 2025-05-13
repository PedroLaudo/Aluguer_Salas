using System;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Models // Certifique-se que o namespace está correto
{
    public class AluguerViewModel
    {
        // Informações da Sala (para mostrar ao utilizador)
        public int SalaId { get; set; }
        public string NomeSala { get; set; }
        public int Capacidade { get; set; }
        public string DescricaoSala { get; set; }

        // Campos do formulário para a Reserva
        [Required(ErrorMessage = "A data da reserva é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Reserva")]
        public DateTime Data { get; set; } = DateTime.Today; // Valor padrão

        [Required(ErrorMessage = "A hora de início é obrigatória.")]
        [DataType(DataType.Time)] // Isto ajudará o browser a mostrar um seletor de hora
        [Display(Name = "Hora de Início")]
        public DateTime HoraInicio { get; set; }

        [Required(ErrorMessage = "A hora de fim é obrigatória.")]
        [DataType(DataType.Time)] // Isto ajudará o browser a mostrar um seletor de hora
        [Display(Name = "Hora de Fim")]
        public DateTime HoraFim { get; set; }

        // Não precisamos incluir aqui IdReserva, Status, ou UtilizadorIdentityId
        // porque eles serão definidos no controller ou pela base de dados.
    }
}