using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Models;

namespace PhishGuard.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Alvo> Alvos { get; set; }
    }
}