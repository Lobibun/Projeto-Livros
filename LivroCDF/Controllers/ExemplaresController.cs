using LivroCDF.Models;
using LivroCDF.Models.Enums;
using LivroCDF.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using LivroCDF.Data;
using Microsoft.EntityFrameworkCore;

namespace LivroCDF.Controllers
{
    public class ExemplaresController : Controller
    {
        private readonly LivroService _service;
        private readonly LivrariaContext _context;
        public ExemplaresController(LivroService service, LivrariaContext context)
        {
            _context = context;
            _service = service;
        }
        public async Task<IActionResult> Index(string termoPesquisa, StatusLivro? filtoStatus)
        {
            var consulta = _context.Exemplares.Include(e => e.Livro).AsQueryable();
            consulta = consulta.Where(x => x.Status != StatusLivro.Vendido);

            if (!string.IsNullOrEmpty(termoPesquisa))
            {
                consulta = consulta.Where(x => x.Livro.Titulo.Contains(termoPesquisa));
            }

            if (filtoStatus.HasValue)
            {
                consulta = consulta.Where(x => x.Status == filtoStatus.Value);
            }

            ViewBag.TermoPesquisa = termoPesquisa;
            ViewBag.FiltoStatus = filtoStatus;

            return View(await consulta.ToListAsync());
        }

        public async Task<IActionResult> Vendas(DateTime? dataInicio, DateTime? dataFim)
        {
            var consulta = _context.Exemplares
                .Include(e => e.Livro)
                .Where(x => x.Status == StatusLivro.Vendido);

            if (dataInicio.HasValue)
            {
                consulta = consulta.Where(x => x.DataEntrada >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                var fimDoDia = dataFim.Value.Date.AddDays(1).AddTicks(-1);

                consulta = consulta.Where(x => x.DataEntrada <= fimDoDia);
            }

 
            consulta = consulta.OrderByDescending(x => x.DataEntrada);

           
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            return View(await consulta.ToListAsync());
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

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome", exemplar.ClienteId);
            return View(exemplar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exemplar exemplar)
        {
            if (id != exemplar.Id) return NotFound();

            // VOCÊ JÁ TINHA ISSO
            ModelState.Remove("Livro");

            // ADICIONE ESTA LINHA ABAIXO:
            ModelState.Remove("Cliente");

            if (ModelState.IsValid)
            {
                try
                {
                    exemplar.DataUltimaAtualizacao = DateTime.Now;

                    if (exemplar.Status == StatusLivro.Vendido)
                    {
                        if (exemplar.DataVenda == null)
                        {
                            exemplar.DataVenda = DateTime.Now;
                        }
                    }
                    else
                    {
                        exemplar.DataVenda = null;
                    }

                    await _service.AtualizarExemplarAsync(exemplar);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Sugestão: Logar o erro aqui ou retornar uma view de erro
                    throw;
                }
            }

            // Se chegar aqui, recarrega os dados para a DropDownList novamente
            ViewBag.TituloLivro = exemplar.Livro?.Titulo; // Pode estar nulo aqui, cuidado

            // É importante recarregar as listas (ViewData/ViewBag) se a validação falhar
            // O select do Cliente precisa ser repopulado:
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome", exemplar.ClienteId);

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

        [HttpGet]
        public async Task<IActionResult> FiltrarEstoque(string termoPesquisa, StatusLivro? filtroStatus)
        {
            var consulta = _context.Exemplares.Include(e => e.Livro).AsQueryable();
            consulta = consulta.Where(x => x.Status != StatusLivro.Vendido);

            if(!string.IsNullOrEmpty(termoPesquisa))
            {
                consulta = consulta.Where(x => x.Livro.Titulo.Contains(termoPesquisa));
            }

            if (filtroStatus.HasValue)
            {
                consulta = consulta.Where(x => x.Status == filtroStatus.Value);
            }

            return PartialView("_TabelaEstoque", await consulta.ToListAsync());
        }
    }
}
