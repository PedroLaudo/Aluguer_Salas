using System.ComponentModel.DataAnnotations;
using Aluguer_Salas.Models;

using Aluguer_Salas.Data;

namespace Aluguer_Salas.Models
{
    public class Limpeza
    {
        /// <summary>
        /// Identificador da sala
        /// </summary>
        public int IdSala { get; set; }

        /// <summary>
        /// Identificador do utilizador responsável pela limpeza
        /// </summary>
        public int IdUtilizador { get; set; }

        /// <summary>
        /// Dia da semana em que a limpeza é realizada
        /// </summary>
        public string DiaSemana { get; set; }

        // Relacionamento com a tabela Salas
        public Sala Sala { get; set; }

        // Relacionamento com a tabela Funcionario
        public Funcionario Funcionario { get; set; }
    }
}
