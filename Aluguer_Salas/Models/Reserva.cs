using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Models
{
    public class Reserva
    {
        /// <summary>
        /// Identificador da reserva
        /// </summary>
        [Key]
        public int IdReserva { get; set; }

        /// <summary>
        /// Data e hora de início da reserva
        /// </summary>
        [Required]
        public DateTime HoraInicio { get; set; }

        /// <summary>
        /// Data e hora de fim da reserva
        /// </summary>
        [Required]
        public DateTime HoraFim { get; set; }

        [Required]
        public DateTime Data { get; set; }

        /// <summary>
        /// Status da reserva
        /// </summary>
        [Required]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Chave estrangeira para a Sala
        /// </summary>
        [Required]
        public int IdSala { get; set; }

        [ForeignKey("IdSala")]
        public Sala? Sala { get; set; }

        [Required]
        public string UtilizadorIdentityId { get; set; } = string.Empty;

        [ForeignKey("UtilizadorIdentityId")]
        public Utilizador? Utilizador { get; set; }
    }
}
