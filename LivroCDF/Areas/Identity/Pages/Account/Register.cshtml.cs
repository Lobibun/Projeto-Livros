using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;             
using LivroCDF.Models;        

namespace LivroCDF.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUserStore<Usuario> _userStore;
        private readonly IUserEmailStore<Usuario> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<Usuario> userManager,
            IUserStore<Usuario> userStore,
            SignInManager<Usuario> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            // Sugestão: Adicione Nome, já que criamos no Model
            [Required(ErrorMessage = "O Nome é obrigatório")]
            [Display(Name = "Nome Completo")]
            public string Nome { get; set; }

            public string? FotoCaminho { get; set; }

            [NotMapped]
            [Display(Name = "Foto de Perfil")]
            public IFormFile? ArquivoFoto { get; set; } // O arquivo vem aqui

            [NotMapped]
            public bool RemoverFoto { get; set; }

            [Required(ErrorMessage = "O Email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A Senha é obrigatória")]
            [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "As senhas não conferem.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // MUDANÇA 2: Passamos os dados extras para o objeto User
                user.Nome = Input.Nome;

                // MUDANÇA 3: Lógica de salvar a foto
                if (Input.ArquivoFoto != null)
                {
                    // Define pasta física: wwwroot/imagens/usuarios
                    string pastaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens", "usuarios");

                    if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                    // Cria nome único para não substituir fotos de outros
                    string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(Input.ArquivoFoto.FileName);

                    // Salva no disco
                    using (var stream = new FileStream(Path.Combine(pastaDestino, nomeArquivo), FileMode.Create))
                    {
                        await Input.ArquivoFoto.CopyToAsync(stream);
                    }

                    // Salva o caminho relativo no banco
                    user.FotoCaminho = $"/imagens/usuarios/{nomeArquivo}";
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin criou uma nova conta.");
                    await _userManager.AddToRoleAsync(user, "Comum");
                    return RedirectToAction("Index", "Usuarios");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        // MUDANÇA 4: CreateUser deve retornar um 'Usuario'
        private Usuario CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Usuario>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Usuario)}'.");
            }
        }

        private IUserEmailStore<Usuario> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Usuario>)_userStore;
        }
    }
}