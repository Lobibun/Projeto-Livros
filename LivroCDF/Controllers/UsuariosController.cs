using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")] // Só a chefia entra aqui
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsuariosController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UsuarioViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UsuarioViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // SÓ O CEO PODE PROMOVER
        public async Task<IActionResult> TornarAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _userManager.RemoveFromRoleAsync(user, "Comum"); 
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Trava de segurança no servidor
        public async Task<IActionResult> ResetarSenha(string userId)
        {
            // Verificação extra manual
            if (!User.IsInRole("SuperAdmin"))
            {
                TempData["Erro"] = "Apenas o CEO pode resetar senhas.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Remove a senha antiga e coloca a padrão
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var removeResult = await _userManager.RemovePasswordAsync(user);

            if (removeResult.Succeeded)
            {
                // A NOVA SENHA PADRÃO SERÁ: Livraria@123
                var addResult = await _userManager.AddPasswordAsync(user, "Livraria@123");
                if (addResult.Succeeded)
                {
                    TempData["Sucesso"] = $"Senha de {user.Email} resetada para: Livraria@123";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RebaixarParaComum(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Verifica se não está tentando rebaixar outro SuperAdmin
                if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                {
                    TempData["Erro"] = "Você não pode rebaixar um CEO.";
                    return RedirectToAction("Index");
                }

                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "Comum");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(string userId)
        {
            var usuarioAlvo = await _userManager.FindByIdAsync(userId);
            if (usuarioAlvo == null) return NotFound();

            // --- REGRAS DE PODER ---

            // 1. Ninguém pode se auto-deletar por aqui (segurança)
            if (usuarioAlvo.UserName == User.Identity.Name)
            {
                TempData["Erro"] = "Você não pode excluir a si mesmo!";
                return RedirectToAction("Index");
            }

            // 2. Verifica se quem está tentando excluir é CEO ou Admin
            bool souCEO = User.IsInRole("SuperAdmin");
            bool alvoEhAdmin = await _userManager.IsInRoleAsync(usuarioAlvo, "Admin");
            bool alvoEhCEO = await _userManager.IsInRoleAsync(usuarioAlvo, "SuperAdmin");

            // Se eu sou apenas Admin, NÃO posso apagar outro Admin nem o CEO
            if (!souCEO && (alvoEhAdmin || alvoEhCEO))
            {
                TempData["Erro"] = "Permissão Negada: Gerentes não podem apagar outros Gerentes ou o CEO.";
                return RedirectToAction("Index");
            }

            // Se passou pelas regras, apaga
            await _userManager.DeleteAsync(usuarioAlvo);
            return RedirectToAction("Index");
        }
    }

    public class UsuarioViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}