using System.ComponentModel.DataAnnotations;

namespace LivroCDF.Models.Enums
{
    public enum StatusLivro : int
    {
        [Display(Name = "Em Estoque")]
        Estoque = 0,
        [Display(Name = "A Pagar")]
        APagar = 1,
        [Display(Name = "Vendido")]
        Vendido = 2
    }
}
