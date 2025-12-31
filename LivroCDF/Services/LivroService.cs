using LivroCDF.Data;
using LivroCDF.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivroCDF.Services
{
    public class LivroService
    {
        private readonly LivrariaContext _context;

        public LivroService(LivrariaContext context)
        {
            _context = context;
        }
        public async Task<List<Livro>> FindAllAsync()
        {
            return await _context.Livros
                .Include(x => x.Exemplares)
                .ToListAsync();
        }
        public async Task IsertAsync(Livro obj)
        {
            _context.Add(obj);
            await _context.SaveChangesAsync();
        }

    }
}
