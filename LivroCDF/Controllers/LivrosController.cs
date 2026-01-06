using LivroCDF.Data;
using LivroCDF.Models;
using LivroCDF.Models.Enums;
using LivroCDF.Services;
using LivroCDF.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Necessário para salvar arquivos
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http; // Necessário para baixar do Google
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    [Authorize]
    public class LivrosController : Controller
    {
        private readonly LivroService _livroService;
        private readonly LivrariaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Para acessar a pasta wwwroot

        public LivrosController(LivroService livroService, LivrariaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _livroService = livroService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _livroService.FindAllAsync();
            return View(list);
        }

        // GET: Create
        public IActionResult Create()
        {
            // Retornamos a ViewModel vazia
            return View(new LivroFormViewModel());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LivroFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Converte ViewModel para Entidade Livro
                var livro = new Livro
                {
                    Titulo = viewModel.Titulo,
                    Autor = viewModel.Autor,
                    ISBN = viewModel.ISBN,
                    Valor = viewModel.Valor,
                    Status = viewModel.Status,
                    // Definimos DataVenda null ou logica especifica se necessario
                };

                // --- LÓGICA DA FOTO ---
                livro.FotoCaminho = await ProcessarFoto(viewModel.ArquivoFoto, viewModel.CapaUrlExterna);

                await _livroService.InsertAsync(livro);

                // Auditoria
                await RegistrarLog("Cadastro de Título", $"Cadastrou o novo livro: {livro.Titulo}");

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var livro = await _livroService.FindByIdAsync(id.Value);
            if (livro == null) return NotFound();

            // Preenche a ViewModel com os dados do Banco
            var viewModel = new LivroFormViewModel
            {
                Id = livro.Id,
                Titulo = livro.Titulo,
                Autor = livro.Autor,
                ISBN = livro.ISBN,
                Valor = livro.Valor,
                Status = livro.Status,
                FotoCaminhoAtual = livro.FotoCaminho // Passa a foto atual para a View ver
            };

            return View(viewModel);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LivroFormViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Busca o livro original no banco para não perder dados e para pegar a foto antiga
                    var livroOriginal = await _livroService.FindByIdAsync(id);
                    if (livroOriginal == null) return NotFound();

                    // Atualiza os dados de texto
                    livroOriginal.Titulo = viewModel.Titulo;
                    livroOriginal.Autor = viewModel.Autor;
                    livroOriginal.ISBN = viewModel.ISBN;
                    livroOriginal.Valor = viewModel.Valor;
                    livroOriginal.Status = viewModel.Status;

                    // --- LÓGICA DA FOTO NA EDIÇÃO ---
                    // Se o usuário mandou arquivo novo OU link novo, processamos.
                    // Se ele deixou tudo vazio, mantemos a foto antiga (livroOriginal.FotoCaminho).
                    if (viewModel.ArquivoFoto != null || !string.IsNullOrEmpty(viewModel.CapaUrlExterna))
                    {
                        livroOriginal.FotoCaminho = await ProcessarFoto(viewModel.ArquivoFoto, viewModel.CapaUrlExterna);
                    }

                    await _livroService.UpdateAsync(livroOriginal);
                    await RegistrarLog("Edição de Título", $"Editou dados do livro ID {livroOriginal.Id}: {livroOriginal.Titulo}");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }
        private async Task<string?> ProcessarFoto(IFormFile? arquivo, string? urlExterna)
        {
            string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "imagens", "livros");
            if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

            if (arquivo != null)
            {
                string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(arquivo.FileName);
                string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }
                return "/imagens/livros/" + nomeArquivo;
            }

            else if (!string.IsNullOrEmpty(urlExterna))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(urlExterna);
                        string nomeArquivo = Guid.NewGuid().ToString() + ".jpg"; 
                        string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

                        await System.IO.File.WriteAllBytesAsync(caminhoCompleto, imageBytes);
                        return "/imagens/livros/" + nomeArquivo;
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null; 
        }

        private async Task RegistrarLog(string acao, string detalhes)
        {
            var log = new LogAuditoria
            {
                Usuario = User.Identity.Name ?? "Desconhecido",
                Acao = acao,
                Detalhes = detalhes,
                DataAcao = DateTime.Now
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
        }
        public async Task<IActionResult> Details(int? id, string filtroStatus, DateTime? dataInicio, DateTime? dataFim)
        {
            if (id == null) return NotFound();

            var livro = await _context.Livros
                .Include(l => l.Exemplares)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (livro == null) return NotFound();

            var itensFiltrados = livro.Exemplares.AsQueryable();

            if (!string.IsNullOrEmpty(filtroStatus))
            {
                if (Enum.TryParse<StatusLivro>(filtroStatus, true, out var statusEnum))
                {
                    itensFiltrados = itensFiltrados.Where(i => i.Status == statusEnum);
                }
            }

            // 2. Filtro por Data
            if (dataInicio.HasValue)
                itensFiltrados = itensFiltrados.Where(i => i.DataEntrada >= dataInicio.Value);

            if (dataFim.HasValue)
                itensFiltrados = itensFiltrados.Where(i => i.DataEntrada <= dataFim.Value);

            // 3. Ordenação e atribuição
            livro.Exemplares = itensFiltrados.OrderByDescending(i => i.DataEntrada).ToList();

            // Mantém os dados na tela
            ViewData["FiltroStatus"] = filtroStatus;
            ViewData["DataInicio"] = dataInicio?.ToString("yyyy-MM-dd");
            ViewData["DataFim"] = dataFim?.ToString("yyyy-MM-dd");

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
            var livro = await _livroService.FindByIdAsync(id);
            if (livro == null) return NotFound();

            if (livro.Exemplares != null && livro.Exemplares.Any())
            {
                ViewData["ErroExclusao"] = "Não é possível excluir este Título pois existem exemplares vinculados.";
                return View(livro);
            }

            string tituloApagado = livro.Titulo;
            await _livroService.RemoveAsync(id);
            await RegistrarLog("Exclusão de Título", $"Removeu o título: {tituloApagado} (ID: {id})");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarCatalogo(string termo)
        {
            var query = _context.Livros
                        .Include(l => l.Exemplares)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(termo))
            {
                query = query.Where(l => l.Titulo.Contains(termo) ||
                                         l.Autor.Contains(termo) ||
                                         l.ISBN.Contains(termo));
            }

            var livrosFiltrados = await query.ToListAsync();

            return PartialView("_TabelaLivros", livrosFiltrados);
        }
    }
}