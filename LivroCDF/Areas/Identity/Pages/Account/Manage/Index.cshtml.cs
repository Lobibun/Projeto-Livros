using System.ComponentModel.DataAnnotations;
using LivroCDF.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LivroCDF.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public IndexModel(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Nome Completo")]
            public string? Nome { get; set; }

            [Phone]
            [Display(Name = "Telefone")]
            public string? PhoneNumber { get; set; }

            public string? FotoCaminho { get; set; } // Apenas para leitura/exibição

            [Display(Name = "Foto de Perfil")]
            public IFormFile? ArquivoFoto { get; set; } // Para upload
        }

        private async Task LoadAsync(Usuario user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Nome = user.Nome,
                PhoneNumber = phoneNumber,
                FotoCaminho = user.FotoCaminho
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Usuário não encontrado.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Usuário não encontrado.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (user.Nome != Input.Nome) user.Nome = Input.Nome;
            if (user.PhoneNumber != Input.PhoneNumber) user.PhoneNumber = Input.PhoneNumber;

            if (Input.ArquivoFoto != null)
            {
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extensao = Path.GetExtension(Input.ArquivoFoto.FileName).ToLower();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    ModelState.AddModelError("Input.ArquivoFoto", "Apenas imagens (.jpg, .png, etc) são permitidas.");
                    await LoadAsync(user);
                    return Page();
                }

                string pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens", "usuarios");
                if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

                string nomeArquivo = Guid.NewGuid().ToString() + extensao;
                string caminhoCompleto = Path.Combine(pasta, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await Input.ArquivoFoto.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.FotoCaminho))
                {
                    string caminhoAntigo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.FotoCaminho.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoAntigo)) System.IO.File.Delete(caminhoAntigo);
                }

                user.FotoCaminho = $"/imagens/usuarios/{nomeArquivo}";
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Seu perfil foi atualizado!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoverFotoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.FotoCaminho))
            {
                string caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.FotoCaminho.TrimStart('/'));
                if (System.IO.File.Exists(caminhoFisico))
                {
                    System.IO.File.Delete(caminhoFisico);
                }

                user.FotoCaminho = null; 
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user); 
            }

            StatusMessage = "Foto removida com sucesso.";
            return RedirectToPage();
        }
    }
}