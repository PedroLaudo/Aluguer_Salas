using System.Collections.Generic; // ADICIONE ESTE USING
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aluguer_Salas.Models;
// ADICIONE ESTE USING SE A CLASSE Limpeza ESTIVER EM OUTRO NAMESPACE (ex: Aluguer_Salas.Models)
// using Aluguer_Salas.Models; 

namespace Aluguer_Salas.Data
{
    public class Funcionario
    {
        [Key]
        public int FuncionarioId { get; set; }

        [Required]
        public string UtilizadorId { get; set; }

        [ForeignKey("UtilizadorId")]
        public virtual Utilizador Utilizador { get; set; }

        // public string Cargo { get; set; }

        // ADICIONE ESTA PROPRIEDADE
        public virtual ICollection<Limpeza> Limpezas { get; set; } = new List<Limpeza>();
    }
}