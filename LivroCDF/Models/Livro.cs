using System.ComponentModel.DataAnnotations;
namespace LivroCDF.Models
{
    public class Livro
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public float Valor { get; set; }
        public string Titulo { get; set; }
        public string Autor {  get; set; }
        public string ISBN { get; set; }
        public string Status { get; set; }
        public DateTime? DataVenda { get; set; }
        public int? ClienteId { get; set; } 
        public Cliente? Cliente { get; set; }
        public ICollection<Exemplar> Exemplares { get; set; } = new List<Exemplar>();
    }
}
