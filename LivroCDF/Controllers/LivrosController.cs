using LivroCDF.Models;
using LivroCDF.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LivroCDF.Controllers
{
    public class LivrosController : Controller
    {
        private readonly LivroService _livroService;
        private readonly ClienteService _clienteService;

        // -----------------------------------------------------------
        // 1. CONSTRUTOR CORRIGIDO (Adicionado o ClienteService)
        // -----------------------------------------------------------
        public LivrosController(LivroService livroService, ClienteService clienteService)
        {
            _livroService = livroService;
            _clienteService = clienteService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _livroService.FindAllAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Livro livro)
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("Exemplares");

            if (ModelState.IsValid)
            {
                await _livroService.InsertAsync(livro);
                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            return View(livro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Livro livro)
        {
            if (id != livro.Id) return NotFound();

            ModelState.Remove("Cliente");
            ModelState.Remove("Exemplares");

            if (ModelState.IsValid)
            {
                try
                {
                    await _livroService.UpdateAsync(livro);
                    return RedirectToAction(nameof(Index));
                }
                catch (ApplicationException e)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            var clientes = await _clienteService.FindAllAsync();
            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nome", livro.ClienteId);

            return View(livro);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            return View(livro);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            return View(livro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Precisamos buscar o livro junto com os exemplares para contar
            var livro = await _livroService.FindByIdAsync(id); // O FindByIdAsync já faz o Include

            if (livro == null) return NotFound();

            // VERIFICAÇÃO DE SEGURANÇA
            if (livro.Exemplares != null && livro.Exemplares.Any())
            {
                ViewData["ErroExclusao"] = "Não é possível excluir este Livro pois existem exemplares (físicos) cadastrados no sistema vinculados a ele.";
                return View(livro);
            }

            await _livroService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}