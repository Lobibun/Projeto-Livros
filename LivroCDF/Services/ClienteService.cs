using LivroCDF.Data;
using LivroCDF.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace LivroCDF.Services
{
    public class ClienteService
    {
        private readonly LivrariaContext _context;

        public ClienteService(LivrariaContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> FindAllAsync()
        {
            return await _context.Clientes.OrderBy(x => x.Nome).ToListAsync();
        }
    }
}