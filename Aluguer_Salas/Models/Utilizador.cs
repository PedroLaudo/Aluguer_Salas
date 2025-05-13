using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Aluguer_Salas.Models;
using Aluguer_Salas.Data;

namespace Aluguer_Salas.Models
{
   
    public class Utilizador : IdentityUser
    {
        
        [Required(ErrorMessage = "O {0} � de preenchimento obrigat�rio.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no m�ximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

       
        [StringLength(255, ErrorMessage = "O {0} deve ter no m�ximo {1} caracteres.")]
        public string? Login { get; set; }

        public virtual Utente? Utente { get; set; }

        //public string UtilizadorIdentityId { get; set; } = null!; 

    }
}
