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

        public async Task<Livro> FindByIdAsync(int id)
        {
            return await _context.Livros
                .Include(obj => obj.Exemplares)
                .FirstOrDefaultAsync(obj => obj.Id == id);
        }

        public async Task InsertAsync(Livro obj)
        {
            _context.Add(obj);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Livro obj)
        {
            bool exists = await _context.Livros.AnyAsync(x => x.Id == obj.Id);
            if (!exists)
            {
                throw new KeyNotFoundException("Livro não encontrado");
            }
            _context.Update(obj);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var obj = await _context.Livros.FindAsync(id);
            if (obj != null)
            {
                _context.Livros.Remove(obj);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Exemplar>> BuscarTodosExemplaresAsync()
        {
            return await _context.Exemplares
                .Include(exemplar => exemplar.Livro)
                .ToListAsync();
        }

        public async Task<Exemplar> BuscarExemplarPorIdAsync(int id)
        {
            return await _context.Exemplares
                .Include(x => x.Livro)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task InserirExemplarAsync(Exemplar exemplar)
        {
            if (exemplar.Valor == 0)
            {
                var livroPai = await _context.Livros.FindAsync(exemplar.LivroId);
                if (livroPai != null)
                {
                    exemplar.Valor = livroPai.Valor;
                }
            }
            _context.Add(exemplar);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarExemplarAsync(Exemplar exemplar)
        {
            bool existe = await _context.Exemplares.AnyAsync(x => x.Id == exemplar.Id);
            if (!existe) return;

            _context.Update(exemplar);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverExemplarAsync(int id)
        {
            var exemplar = await _context.Exemplares.FindAsync(id);
            if (exemplar != null)
            {
                _context.Exemplares.Remove(exemplar);
                await _context.SaveChangesAsync();
            }
        }
    }
}