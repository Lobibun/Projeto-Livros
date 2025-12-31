using System.ComponentModel.DataAnnotations;

namespace LivroCDF.Models.Enums
{
    public enum StatusLivro : int
    {
        [Display(Name = "Em Estoque")]
        Estoque,
        [Display(Name = "A Pagar")]
        APagar,
        [Display(Name = "Vendido")]
        Vendido
    }
}
