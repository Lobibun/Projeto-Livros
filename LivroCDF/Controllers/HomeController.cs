using System.Diagnostics;
using LivroCDF.Models;
using LivroCDF.ViewModels; // Para o HomeViewModel
using LivroCDF.Data;       // Para o ApplicationDbContext
using LivroCDF.Models.Enums; // IMPORTANTE: Para acessar StatusLivro
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LivroCDF.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LivrariaContext _context;

        public HomeController(ILogger<HomeController> logger, LivrariaContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var dashboard = new HomeViewModel();

            dashboard.TotalLivrosCadastrados = _context.Livros.Count();
            dashboard.TotalClientes = _context.Clientes.Count();
            dashboard.QtdEmEstoque = _context.Exemplares
                                        .Count(e => e.Status == StatusLivro.Estoque);

            dashboard.QtdVendidos = _context.Exemplares
                                        .Count(e => e.Status == StatusLivro.Vendido);

            var exemplaresAPagar = _context.Exemplares
                                        .Where(e => e.Status == StatusLivro.APagar);

            dashboard.QtdAPagar = exemplaresAPagar.Count();
            dashboard.ValorTotalAPagar = exemplaresAPagar.Sum(e => e.Valor);

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}