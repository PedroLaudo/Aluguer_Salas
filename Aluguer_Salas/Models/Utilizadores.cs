using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Data
{
   
    public class Utilizadores : IdentityUser
    {
        
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;





        //public string UtilizadorIdentityId { get; set; } = null!; 

    }
}
