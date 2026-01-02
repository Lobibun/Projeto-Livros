using LivroCDF.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LivroCDF.Models;

namespace LivroCDF.Controllers
{
    public class ClientesController : Controller
    {
        private readonly LivrariaContext _context;

        public ClientesController(LivrariaContext context)
        {
            _context = context;
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
                _context.Add(cliente);
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
                    _context.Update(cliente);
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
                    .Include(c => c.Compras) // Carrega as compras para verificar
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cliente == null) return NotFound();

                // VERIFICAÇÃO DE SEGURANÇA
                if (cliente.Compras != null && cliente.Compras.Any())
                {
                    // Envia uma mensagem de erro para a tela
                    ViewData["ErroExclusao"] = "Não é possível excluir este cliente pois ele possui compras registradas. O histórico financeiro não pode ser apagado.";
                    return View(cliente); // Retorna para a mesma tela mostrando o erro
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        
        

        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }
     }
}
