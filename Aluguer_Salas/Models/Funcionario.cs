using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aluguer_Salas.Models
{
    public class Funcionario
    {
        [Key] // Chave Primária própria do Funcionário
        public int FuncionarioId { get; set; } // Renomeado para clareza, pode ser 'Id'

        // --- Chave Estrangeira para Utilizadores ---
        [Required] // Geralmente um funcionário DEVE estar ligado a um utilizador
        public string UtilizadorId { get; set; } // <<< TIPO STRING, correspondendo a Utilizadores.Id

        // --- Propriedade de Navegação ---
        [ForeignKey("UtilizadorId")] // Diz ao EF que UtilizadorId é a FK para esta navegação
        public virtual Utilizador Utilizador { get; set; } // 'virtual' para lazy loading

        public virtual ICollection<Limpeza> Limpezas { get; set; } = new List<Limpeza>();

        // Outras propriedades do Funcionário (ex: Cargo, DataAdmissao, etc.)
        // public string Cargo { get; set; }
    }
}