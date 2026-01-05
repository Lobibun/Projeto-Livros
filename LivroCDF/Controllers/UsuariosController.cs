using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LivroCDF.Models;

namespace LivroCDF.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuariosController(UserManager<Usuario> userManager)
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
                    Nome = user.Nome,
                    FotoCaminho = user.FotoCaminho,
                    Roles = roles
                });
            }

            return View(userRolesViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> TornarAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                if (await _userManager.IsInRoleAsync(user, "Comum"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Comum");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetarSenha(string userId)
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                TempData["Erro"] = "Apenas o CEO pode resetar senhas.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var removeResult = await _userManager.RemovePasswordAsync(user);

            if (removeResult.Succeeded)
            {
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

            if (usuarioAlvo.UserName == User.Identity.Name)
            {
                TempData["Erro"] = "Você não pode excluir a si mesmo!";
                return RedirectToAction("Index");
            }

            bool souCEO = User.IsInRole("SuperAdmin");
            bool alvoEhAdmin = await _userManager.IsInRoleAsync(usuarioAlvo, "Admin");
            bool alvoEhCEO = await _userManager.IsInRoleAsync(usuarioAlvo, "SuperAdmin");

            if (!souCEO && (alvoEhAdmin || alvoEhCEO))
            {
                TempData["Erro"] = "Permissão Negada: Gerentes não podem apagar outros Gerentes ou o CEO.";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(usuarioAlvo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarUsuarios(string termo)
        {
            var usuariosQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(termo))
            {
                termo = termo.ToLower();
                usuariosQuery = usuariosQuery.Where(u => u.UserName.ToLower().Contains(termo) ||
                                                         u.Email.ToLower().Contains(termo));
            }

            var usuarios = await usuariosQuery.ToListAsync();
            var listaViewModel = new List<UsuarioViewModel>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                listaViewModel.Add(new UsuarioViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Nome = user.Nome,
                    FotoCaminho = user.FotoCaminho,
                    Roles = roles
                });
            }

            return PartialView("_TabelaUsuarios", listaViewModel);
        }

    }

    public class UsuarioViewModel
    {
        public string UserId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string FotoCaminho { get; set; }
        public IList<string> Roles { get; set; }
    }
}