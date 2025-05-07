using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Para [ForeignKey]

namespace Aluguer_Salas.Data
{
    public class Utentes:Utilizadores
    {
        //[Key]
        //public int Id { get; set; } // Chave primária da tabela Utentes (era IdUtilizador, mudei para Id para clareza)

        //[Required]
        //[StringLength(60, ErrorMessage = "O {0} deve ter no máximo {1} caracteres.")]
        //public string Email { get; set; } = string.Empty;

        [Required] // Se o tipo é sempre obrigatório
        public string Tipo { get; set; } = "Aluno"; // "Aluno", "Professor", etc.

        // Chave Estrangeira para a tabela Utilizadores (AspNetUsers)
      //  [Required]
        
        //public string UtilizadorIdentityId { get; set; } = null!; // Deve ser esta

        //// Propriedade de navegação de volta para a entidade Utilizadores (Identity)
        //[ForeignKey("UtilizadorIdentityId")] 
        //public virtual Utilizadores Utilizador { get; set; } = null!;

        // REMOVI: public string UtilizadorId { get; set; }  // FK para o AspNetUser
        // A propriedade UtilizadorIdentityId já cumpre essa função.
    }
}