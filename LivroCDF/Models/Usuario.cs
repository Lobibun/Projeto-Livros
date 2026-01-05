using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LivroCDF.Models
{
    public class Usuario : IdentityUser
    {
        public string? Nome { get; set; }

        public string? FotoCaminho { get; set; }

        [NotMapped]
        [Display(Name = "Foto do Perfil")]
        public IFormFile? ArquivoFoto { get; set; }

        [NotMapped]
        public bool RemoverFoto { get; set; }
    }
}