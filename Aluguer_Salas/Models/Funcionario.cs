using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aluguer_Salas.Models
{
    public class Funcionario
    {
        [Key] // Chave Prim�ria pr�pria do Funcion�rio
        public int FuncionarioId { get; set; } // Renomeado para clareza, pode ser 'Id'

        // --- Chave Estrangeira para Utilizadores ---
        [Required] // Geralmente um funcion�rio DEVE estar ligado a um utilizador
        public string UtilizadorId { get; set; } // <<< TIPO STRING, correspondendo a Utilizadores.Id

        // --- Propriedade de Navega��o ---
        [ForeignKey("UtilizadorId")] // Diz ao EF que UtilizadorId � a FK para esta navega��o
        public virtual Utilizador Utilizador { get; set; } // 'virtual' para lazy loading

        public virtual ICollection<Limpeza> Limpezas { get; set; } = new List<Limpeza>();

        // Outras propriedades do Funcion�rio (ex: Cargo, DataAdmissao, etc.)
        // public string Cargo { get; set; }
    }
}