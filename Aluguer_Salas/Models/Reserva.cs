using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Models
{
    public class Reserva
    {
        
        [Key]
        public int IdReserva { get; set; }

      
        [Required]
        public DateTime HoraInicio { get; set; }

       
        [Required]
        public DateTime HoraFim { get; set; }

        [Required]
        public DateTime Data { get; set; }

       
        [Required]
        public string Status { get; set; } = string.Empty;

    
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
