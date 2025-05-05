using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Data
{
    // A classe herda de IdentityUser<string>, que já tem um Id tipo string (GUID)
    public class Utilizadores : IdentityUser
    {
        // Nome completo do utilizador (campo personalizado)
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [StringLength(255, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        // Podes manter o campo Login se quiseres usá-lo como apelido/username alternativo
        [StringLength(255, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        public string? Login { get; set; }

        // Não guardar passwords diretamente!
        // A gestão da password é feita com o Identity (PasswordHash, métodos de autenticação, etc.)
        // Portanto, removemos esta propriedade:
        // public string Password { get; set; }
    }
}
