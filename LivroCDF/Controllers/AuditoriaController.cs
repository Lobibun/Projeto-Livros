using LivroCDF.Data;
using LivroCDF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(DateTime? minDate, DateTime? maxDate, string tipoAcao, string funcionario) // <--- Novo parâmetro
        {
            var query = _context.LogsAuditoria.AsQueryable();

            if (minDate.HasValue) query = query.Where(l => l.DataAcao >= minDate.Value);
            if (maxDate.HasValue)
            {
                var dataFim = maxDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.DataAcao <= dataFim);
            }

            if (!string.IsNullOrEmpty(tipoAcao)) query = query.Where(l => l.Acao == tipoAcao);

            if (!string.IsNullOrEmpty(funcionario))
            {
                query = query.Where(l => l.Usuario == funcionario);
            }

            var listaAcoes = await _context.LogsAuditoria.Select(l => l.Acao).Distinct().OrderBy(a => a).ToListAsync();
            ViewBag.ListaAcoes = new SelectList(listaAcoes, tipoAcao);

            var listaUsuarios = await _context.LogsAuditoria.Select(l => l.Usuario).Distinct().OrderBy(u => u).ToListAsync();
            ViewBag.ListaUsuarios = new SelectList(listaUsuarios, funcionario); 

            ViewBag.MinDate = minDate?.ToString("yyyy-MM-dd");
            ViewBag.MaxDate = maxDate?.ToString("yyyy-MM-dd");

            var logs = await query.OrderByDescending(l => l.DataAcao).ToListAsync();
            return View(logs);
        }
    }
}