using LivroCDF.Models;
using Microsoft.EntityFrameworkCore;

namespace LivroCDF.Data
{
    public class LivrariaContext : DbContext
    {
        public LivrariaContext(DbContextOptions<LivrariaContext> options) : base(options) 
        { }

        public DbSet<Livro> Livros { get; set; }
        public DbSet<Exemplar> Exemplares { get; set; }
    }
}
