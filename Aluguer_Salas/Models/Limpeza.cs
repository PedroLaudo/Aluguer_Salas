using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aluguer_Salas.Data
{

    // definir aqui a PK
    public class Limpeza
    {/// <summary>
        /// Dia da semana em que a limpeza � realizada
        /// </summary>
        public string DiaSemana { get; set; }

        // hora limpeza


        /// <summary>
        /// Identificador da sala
        /// </summary>
        [ForeignKey(nameof(Sala))]
        public int IdSala { get; set; }
        // Relacionamento com a tabela Salas
        public Salas Sala { get; set; }



   /// <summary>
        /// Identificador do utilizador respons�vel pela limpeza
        /// </summary>
        public int IdUtilizador { get; set; }

        // Relacionamento com a tabela Funcionario
        public Funcionario Funcionario { get; set; }
    }
}
