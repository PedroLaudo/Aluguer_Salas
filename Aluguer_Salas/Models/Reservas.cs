using System;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Data
{
    public class Reservas
    {
        /// <summary>
        /// Identificador da reserva
        /// </summary>
        [Key]
        public int IdReserva { get; set; }

        /// <summary>
        /// Identificador do utilizador que fez a reserva
        /// </summary>
        public int IdUtilizador { get; set; }

        /// <summary>
        /// Identificador da sala reservada
        /// </summary>
        public int IdSala { get; set; }

        /// <summary>
        /// Data e hora de início da reserva
        /// </summary>
        public DateTime DataHoraInicio { get; set; }

        /// <summary>
        /// Data e hora de fim da reserva
        /// </summary>
        public DateTime DataHoraFim { get; set; }

        /// <summary>
        /// Status da reserva
        /// </summary>
        public string Status { get; set; } = string.Empty;  // inicializado

        // Relacionamento com a tabela Utilizadores
        public Utilizadores? Utilizador { get; set; }      // pode ser null

        // Relacionamento com a tabela Salas
        public Sala? Sala { get; set; }                    // pode ser null
    }
}
