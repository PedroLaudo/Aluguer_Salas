// Ficheiro: Models/AluguerViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Models
{
    public class AluguerViewModel
    {
        // Detalhes da Sala (para exibição)
        public int SalaId { get; set; }
        public string? NomeSala { get; set; }
        public int Capacidade { get; set; }
        public string? DescricaoSala { get; set; }

        // Inputs do Formulário de Reserva
        [Required(ErrorMessage = "A data da reserva é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today; 

        [Required(ErrorMessage = "A hora de início é obrigatória.")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; } // Mantido como 'HoraInicio'

        [Required(ErrorMessage = "A hora de fim é obrigatória.")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFim { get; set; }    // Mantido como 'HoraFim'

        // Para mostrar horários já ocupados na view
        public List<HorarioOcupadoViewModel> HorariosOcupados { get; set; } = new List<HorarioOcupadoViewModel>();
    }

    public class HorarioOcupadoViewModel // ViewModel auxiliar
    {
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        // Pode adicionar uma string "Descricao" se quiser mostrar quem reservou, etc. (com cuidado pela privacidade)
    }
}