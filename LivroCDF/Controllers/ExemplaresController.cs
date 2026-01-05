using LivroCDF.Models;
using LivroCDF.Models.Enums;
using LivroCDF.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using LivroCDF.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LivroCDF.Controllers
{
    [Authorize]
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

        [HttpGet]
        public async Task<ActionResult> FiltrarVendas(string termo, DateTime? dataInicio, DateTime? dataFim)
        {
            var consulta = _context.Exemplares
                .Include(e => e.Livro)
                .Include(e => e.Cliente)
                .Where(x => x.Status == StatusLivro.Vendido)
                .AsQueryable();

            if (!string.IsNullOrEmpty(termo))
            {
                consulta = consulta.Where(x => x.Livro.Titulo.Contains(termo) ||
                                               (x.Cliente != null && x.Cliente.Nome.Contains(termo)));
            }

            if (dataInicio.HasValue)
            {
                consulta = consulta.Where(x => x.DataVenda >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                var fimdoDia = dataFim.Value.Date.AddDays(1).AddTicks(-1);
                consulta = consulta.Where(x => x.DataVenda <= fimdoDia);
            }
            consulta = consulta.OrderByDescending(x => x.DataVenda);

            return PartialView("_TabelaVendas", await consulta.ToListAsync());
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

            ModelState.Remove("Cliente");

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

                var log = new LogAuditoria
                {
                    Usuario = User.Identity.Name ?? "Desconhecido",
                    Acao = "Entrada de Estoque",
                    Detalhes = $"Adicionou {Quantidade} cópia(s) para o Livro ID: {exemplar.LivroId}",
                    DataAcao = DateTime.Now
                };
                _context.LogsAuditoria.Add(log);
                await _context.SaveChangesAsync();
                // --------------------------------------

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

            var original = await _context.Exemplares
                                         .AsNoTracking()
                                         .Include(e => e.Livro) 
                                         .FirstOrDefaultAsync(x => x.Id == id);

            if (original == null) return NotFound();


            if (original.Status == StatusLivro.Vendido && exemplar.Status != StatusLivro.Vendido)
            {
                if (original.DataVenda.HasValue)
                {
                    var diasPassados = (DateTime.Now - original.DataVenda.Value).Days;

                    if (diasPassados > 90)
                    {
                        ModelState.AddModelError("Status", $"Não é possível estornar. Prazo de 90 dias expirado ({diasPassados} dias desde a venda).");
                    }
                }
            }

            ModelState.Remove("Livro");
            ModelState.Remove("Cliente");

            if (ModelState.IsValid)
            {
                try
                {
                    exemplar.DataUltimaAtualizacao = DateTime.Now;

                   
                    if ((exemplar.Status == StatusLivro.Vendido || exemplar.Status == StatusLivro.APagar) && exemplar.DataVenda == null)
                    {
                        exemplar.DataVenda = DateTime.Now;
                    }

                   
                    if (exemplar.Status == StatusLivro.Estoque)
                    {
                        exemplar.DataVenda = null;
                        exemplar.ClienteId = null; 
                    }

                    await _service.AtualizarExemplarAsync(exemplar);

                    var log = new LogAuditoria
                    {
                        Usuario = User.Identity.Name ?? "Desconhecido",
                        DataAcao = DateTime.Now
                    };

                    if (exemplar.Status == StatusLivro.Vendido)
                    {
                        log.Acao = "Venda Realizada";
                        log.Detalhes = $"Vendeu o exemplar ID: {exemplar.Id}. Cliente ID: {exemplar.ClienteId ?? 0}";
                    }
                    else if (original.Status == StatusLivro.Vendido && exemplar.Status == StatusLivro.Estoque)
                    {
                        // Log específico para estorno
                        log.Acao = "Estorno / Devolução";
                        log.Detalhes = $"Livro devolvido ao estoque. ID: {exemplar.Id}";
                    }
                    else
                    {
                        log.Acao = "Edição de Exemplar";
                        string nomeStatus = exemplar.Status switch
                        {
                            StatusLivro.APagar => "A Pagar",
                            StatusLivro.Estoque => "Em Estoque",
                            _ => exemplar.Status.ToString()
                        };
                        log.Detalhes = $"Atualizou status do exemplar {exemplar.Id} para {nomeStatus}";
                    }

                    _context.LogsAuditoria.Add(log);
                    await _context.SaveChangesAsync();
                    // ------------------------

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            ViewBag.TituloLivro = original.Livro?.Titulo;
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

            var log = new LogAuditoria
            {
                Usuario = User.Identity.Name ?? "Desconhecido",
                Acao = "Exclusão de Exemplar",
                Detalhes = $"Removeu o exemplar ID: {id} do sistema.",
                DataAcao = DateTime.Now
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();

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
