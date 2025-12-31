using LivroCDF.Models;
using LivroCDF.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    public class ExemplaresController : Controller
    {
        private readonly LivroService _service;
        public ExemplaresController(LivroService service)
        {
            _service = service;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _service.BuscarTodosExemplaresAsync();
            return View(list);
        }
        public async Task<IActionResult> Create()
        {
            var ListLivros = await _service.FindAllAsync();
            ViewBag.LivroId = new SelectList(ListLivros, "Id", "Titulo");
            ViewBag.ListaCompleta = ListLivros;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exemplar exemplar, int Quantidade)
        {
            ModelState.Remove("Livro");
            if (ModelState.IsValid)
            {
                for (int i = 0; i < Quantidade; i++)
                {
                    var novaCopia = new Exemplar
                    {
                        LivroId = exemplar.LivroId,
                        Valor = exemplar.Valor,
                        Status = exemplar.Status,
                        DataEntrada = exemplar.DataEntrada
                    };
                    await _service.InserirExemplarAsync(novaCopia);
                }
                return RedirectToAction(nameof(Index));
            }
            var listLivros = await _service.FindAllAsync();
            ViewBag.LivroId = new SelectList(listLivros, "Id", "Titulo");
            ViewBag.ListaCompleta = listLivros;
            return View(exemplar);

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var exemplar = await _service.BuscarExemplarPorIdAsync(id.Value);
            if (exemplar == null) return NotFound();

            ViewBag.TituloLivro = exemplar.Livro.Titulo;

            return View(exemplar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exemplar exemplar)
        {
            if (id != exemplar.Id) return NotFound();

            ModelState.Remove("Livro");

            if (ModelState.IsValid)
            {
                await _service.AtualizarExemplarAsync(exemplar);
                return RedirectToAction(nameof(Index));
            }
            return View(exemplar);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var exemplar = await _service.BuscarExemplarPorIdAsync(id.Value);
            if (exemplar == null) return NotFound();

            return View(exemplar);

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComfirmado(int id)
        {
            await _service.RemoverExemplarAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
