using LivroCDF.Data;
using LivroCDF.Models;
using LivroCDF.Services; // Certifique-se que o LivroService tem métodos para Insert/Update de LIVRO
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    [Authorize]
    public class LivrosController : Controller
    {
        private readonly LivroService _livroService;
        private readonly LivrariaContext _context;

        public LivrosController(LivroService livroService, LivrariaContext context)
        {
            _livroService = livroService;
            _context = context;
        }

        // GET: Livros
        public async Task<IActionResult> Index()
        {

            var list = await _livroService.FindAllAsync();
            return View(list);
        }

        // GET: Livros/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Livros/Create
        // CORREÇÃO: Agora recebe um LIVRO (Título), não um Exemplar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Livro livro)
        {
            if (ModelState.IsValid)
            {
                // Insere o novo Título no banco
                await _livroService.InsertAsync(livro);

                // --- LOG DE AUDITORIA ---
                var log = new LogAuditoria
                {
                    Usuario = User.Identity.Name ?? "Desconhecido",
                    Acao = "Cadastro de Título",
                    Detalhes = $"Cadastrou o novo livro: {livro.Titulo}",
                    DataAcao = DateTime.Now
                };
                _context.LogsAuditoria.Add(log);
                await _context.SaveChangesAsync();
                // ------------------------

                return RedirectToAction(nameof(Index));
            }
            return View(livro);
        }

        // GET: Livros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            return View(livro);
        }

        // POST: Livros/Edit/5
        // CORREÇÃO: Recebe LIVRO para atualizar dados como Título/Autor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Livro livro)
        {
            if (id != livro.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualiza os dados do Título
                    await _livroService.UpdateAsync(livro);

                    // --- LOG DE AUDITORIA ---
                    var log = new LogAuditoria
                    {
                        Usuario = User.Identity.Name ?? "Desconhecido",
                        Acao = "Edição de Título",
                        Detalhes = $"Editou dados do livro ID {livro.Id}: {livro.Titulo}",
                        DataAcao = DateTime.Now
                    };
                    _context.LogsAuditoria.Add(log);
                    await _context.SaveChangesAsync();
                    // ------------------------

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception) // Catch genérico ou DbConcurrencyException
                {
                    if (await _livroService.FindByIdAsync(id) == null) return NotFound();
                    else throw;
                }
            }
            return View(livro);
        }

        // GET: Livros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            return View(livro);
        }

        // GET: Livros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();
            return View(livro);
        }

        // POST: Livros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var livro = await _livroService.FindByIdAsync(id);
            if (livro == null) return NotFound();

            // --- SEGURANÇA: Não apaga se tiver cópias físicas ---
            if (livro.Exemplares != null && livro.Exemplares.Any())
            {
                ViewData["ErroExclusao"] = "Não é possível excluir este Título pois existem exemplares (ativos ou vendidos) vinculados a ele.";
                return View(livro);
            }

            string tituloApagado = livro.Titulo;

            await _livroService.RemoveAsync(id);

            // --- LOG DE AUDITORIA ---
            var log = new LogAuditoria
            {
                Usuario = User.Identity.Name ?? "Desconhecido",
                Acao = "Exclusão de Título",
                Detalhes = $"Removeu o título: {tituloApagado} (ID: {id})",
                DataAcao = DateTime.Now
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
            // ------------------------

            return RedirectToAction(nameof(Index));
        }
    }
}