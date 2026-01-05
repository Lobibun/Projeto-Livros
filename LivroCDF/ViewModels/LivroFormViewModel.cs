using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LivroCDF.ViewModels
{
    public class LivroFormViewModel
    {
        public int Id { get; set; }
        public float Valor { get; set; }
        [Required(ErrorMessage = "O Título é obrigatório")]
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public string Status { get; set; } = "Disponivel"; 

        [Display(Name = "Upload da Capa")]
        public IFormFile? ArquivoFoto { get; set; } 

        public string? CapaUrlExterna { get; set; } 

        public string? FotoCaminhoAtual { get; set; }
    }
}
