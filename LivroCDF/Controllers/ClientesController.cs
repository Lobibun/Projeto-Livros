using LivroCDF.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LivroCDF.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using LivroCDF.Models.Enums;

namespace LivroCDF.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly LivrariaContext _context;

        public ClientesController(LivrariaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RemoverFoto(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            if (!string.IsNullOrEmpty(cliente.FotoCaminho))
            {
                string caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cliente.FotoCaminho.TrimStart('/'));
                if (System.IO.File.Exists(caminhoFisico))
                {
                    try
                    {
                        System.IO.File.Delete(caminhoFisico);
                    }
                    catch
                    {
                    }
                }
            }

            cliente.FotoCaminho = null;
            _context.Update(cliente);
            await _context.SaveChangesAsync();

            var log = new LogAuditoria
            {
                Usuario = User.Identity.Name ?? "Desconhecido",
                Acao = "Remoção de Foto",
                Detalhes = $"Removeu a foto do cliente ID {id}: {cliente.Nome}",
                DataAcao = DateTime.Now
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = cliente.Id });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Compras)
                .ThenInclude(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                if (cliente.ArquivoFoto != null)
                {
                    string pastaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens");
                    if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                    string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(cliente.ArquivoFoto.FileName);
                    string caminhoArquivo = Path.Combine(pastaDestino, nomeArquivo);

                    using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                    {
                        await cliente.ArquivoFoto.CopyToAsync(stream);
                    }

                    cliente.FotoCaminho = "/imagens/" + nomeArquivo;
                }

                _context.Add(cliente);
                await _context.SaveChangesAsync();

                var log = new LogAuditoria
                {
                    Usuario = User.Identity.Name ?? "Desconhecido",
                    Acao = "Cadastro de Cliente",
                    Detalhes = $"Cadastrou o cliente: {cliente.Nome}",
                    DataAcao = DateTime.Now
                };
                _context.LogsAuditoria.Add(log);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var clienteBanco = await _context.Clientes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (cliente.ArquivoFoto != null)
                    {
                        string pastaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens");
                        if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                        string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(cliente.ArquivoFoto.FileName);
                        string caminhoArquivo = Path.Combine(pastaDestino, nomeArquivo);

                        using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                        {
                            await cliente.ArquivoFoto.CopyToAsync(stream);
                        }

                        cliente.FotoCaminho = "/imagens/" + nomeArquivo;
                    }
                    else
                    {
                        if (clienteBanco != null)
                        {
                            cliente.FotoCaminho = clienteBanco.FotoCaminho;
                        }
                    }

                    _context.Update(cliente);
                    await _context.SaveChangesAsync();

                    // Log
                    var log = new LogAuditoria
                    {
                        Usuario = User.Identity.Name ?? "Desconhecido",
                        Acao = "Edição de Cliente",
                        Detalhes = $"Atualizou dados do cliente ID {cliente.Id}: {cliente.Nome}",
                        DataAcao = DateTime.Now
                    };
                    _context.LogsAuditoria.Add(log);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clientes.Any(e => e.Id == cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Compras)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();

            if (cliente.Compras != null && cliente.Compras.Any())
            {
                ViewData["ErroExclusao"] = "Não é possível excluir este cliente pois ele possui compras registradas. O histórico financeiro não pode ser apagado.";
                return View(cliente);
            }

            string nomeClienteApagado = cliente.Nome;

            if (!string.IsNullOrEmpty(cliente.FotoCaminho))
            {
                string caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cliente.FotoCaminho.TrimStart('/'));
                if (System.IO.File.Exists(caminhoFisico)) System.IO.File.Delete(caminhoFisico);
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            var log = new LogAuditoria
            {
                Usuario = User.Identity.Name ?? "Desconhecido",
                Acao = "Exclusão de Cliente",
                Detalhes = $"Removeu o cliente: {nomeClienteApagado} (ID: {id})",
                DataAcao = DateTime.Now
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Compras)
                .ToListAsync();
            return View(clientes);
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarClientes(string termoPesquisa)
        {
            var consulta = _context.Clientes
                .Include(c => c.Compras)
                .AsQueryable();

            if (!string.IsNullOrEmpty(termoPesquisa))
            {
                consulta = consulta.Where(c => c.Nome.Contains(termoPesquisa) || c.Email.Contains(termoPesquisa));
            }

            return PartialView("_TabelaClientes", await consulta.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarHistorico(int clienteId, string termo, StatusLivro? status, DateTime? dataInicio, DateTime? dataFim)
        {
            var consulta = _context.Exemplares
                                    .Include(e => e.Livro)
                                    .Where(x => x.ClienteId == clienteId)
                                    .AsQueryable();
            if (!string.IsNullOrEmpty(termo))
            {
                consulta = consulta.Where(x => x.Livro.Titulo.Contains(termo));
            }

            if (status.HasValue)
            {
                consulta = consulta.Where(x => x.Status == status.Value);
            }

            if (dataInicio.HasValue)
            {
                consulta = consulta.Where(x => (x.DataVenda ?? x.DataEntrada) == dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                var fimDoDia = dataFim.Value.Date.AddDays(1).AddTicks(-1);
            }

            consulta = consulta.OrderByDescending(x => x.DataVenda).ThenByDescending(x => x.DataEntrada);

            var resultados = await consulta.ToListAsync();
            return PartialView("_HistoricoCliente", resultados);
        }
    }
}