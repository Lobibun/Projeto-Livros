using LivroCDF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LivroCDF.Data
{
    public class LivrariaContext : IdentityDbContext
    {
        public LivrariaContext(DbContextOptions<LivrariaContext> options) : base(options) 
        { }

        public DbSet<Livro> Livros { get; set; }
        public DbSet<Exemplar> Exemplares { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }
    }
}
