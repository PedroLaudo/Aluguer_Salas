using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Data
{
    // A classe herda de IdentityUser<string>, que j� tem um Id tipo string (GUID)
    public class Utilizadores : IdentityUser
    {
        // Nome completo do utilizador (campo personalizado)
        [Required(ErrorMessage = "O {0} � de preenchimento obrigat�rio.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no m�ximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        // Podes manter o campo Login se quiseres us�-lo como apelido/username alternativo
        [StringLength(255, ErrorMessage = "O {0} deve ter no m�ximo {1} caracteres.")]
        public string? Login { get; set; }

        // N�o guardar passwords diretamente!
        // A gest�o da password � feita com o Identity (PasswordHash, m�todos de autentica��o, etc.)
        // Portanto, removemos esta propriedade:
        // public string Password { get; set; }
    }
}
