using LivroCDF.Models;
using LivroCDF.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    public class LivrosController : Controller
    {
        private readonly LivroService _livroService;

        public LivrosController(LivroService livroService)
        {
            _livroService = livroService;
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
            ModelState.Remove("Exemplares");

            if (ModelState.IsValid)
            {
                await _livroService.IsertAsync(livro);
                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            
            var Livro = await _livroService.FindByIdAsync(id.Value);
            if (Livro == null) return NotFound();
            return View(Livro);
        }
    }
}
