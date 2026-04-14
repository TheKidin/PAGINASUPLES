using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Agregamos esta línea
using Microsoft.EntityFrameworkCore;
using PIA.Models;

namespace PIA.Data
{
    // Cambiamos DbContext por IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<ItemCarrito> ItemsCarrito { get; set; }
    }
}
