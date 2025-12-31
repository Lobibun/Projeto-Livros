using LivroCDF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LivroCDF.Models
{
    public class Exemplar
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public float Valor { get; set; }
        public DateTime DataEntrada { get; set; } = DateTime.Now;
        public DateTime? DataSaida { get; set; }
        public StatusLivro Status { get; set; }
        public int LivroId { get; set; }
        public Livro Livro { get; set; }
    }
}
