using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LivroCDF.Models
{
    public class Cliente
    {
        public int Id {get;set;}
        public string? FotoCaminho {get;set;}

        [NotMapped]
        [Display(Name = "Foto de Perfil")]
        public IFormFile? ArquivoFoto { get;set;}
        [NotMapped] 
        public bool RemoverFoto { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        public string Nome { get; set; }

        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string? Email { get; set; }

        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres.")]
        public string? Telefone { get; set; }

        public string? Endereco { get; set; }
        public ICollection<Exemplar>? Compras { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
