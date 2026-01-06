using LivroCDF.Data;
using LivroCDF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LivroCDF.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AuditoriaController : Controller
    {
        private readonly LivrariaContext _context;

        public AuditoriaController(LivrariaContext context)
        {
            _context = context;
        }

        // Ação principal (carrega a página inteira)
        public async Task<IActionResult> Index()
        {
            // Carrega dados iniciais (vazios ou os ultimos 20, conforme preferencia)
            // Aqui carregando os ultimos 50 para a tela não abrir vazia
            var logsIniciais = await _context.LogsAuditoria
                                             .OrderByDescending(l => l.DataAcao)
                                             .Take(50)
                                             .ToListAsync();

            var listaAcoes = await _context.LogsAuditoria
                                           .Select(l => l.Acao)
                                           .Distinct()
                                           .OrderBy(a => a)
                                           .ToListAsync();

            ViewBag.ListaAcoes = new SelectList(listaAcoes);

            return View(logsIniciais);
        }

        // Ação AJAX (Chamada pelo JavaScript em tempo real)
        [HttpGet]
        public async Task<IActionResult> FiltrarLogs(DateTime? minDate, DateTime? maxDate, string tipoAcao, string funcionario)
        {
            var query = _context.LogsAuditoria.AsQueryable();

            if (minDate.HasValue)
                query = query.Where(l => l.DataAcao >= minDate.Value);

            if (maxDate.HasValue)
            {
                var dataFim = maxDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.DataAcao <= dataFim);
            }

            if (!string.IsNullOrEmpty(tipoAcao))
                query = query.Where(l => l.Acao == tipoAcao);

            if (!string.IsNullOrEmpty(funcionario))
            {
                // Busca textual (Contains)
                query = query.Where(l => l.Usuario.Contains(funcionario));
            }

            var logs = await query.OrderByDescending(l => l.DataAcao).ToListAsync();

            // Retorna APENAS o HTML da tabela, não a página toda
            return PartialView("_TabelaLogs", logs);
        }
    }
}