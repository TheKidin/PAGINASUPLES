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
        public DbSet<VarianteProducto> VariantesProducto { get; set; }

        // NUEVA TABLA: El carrito de compras
        public DbSet<ItemCarrito> ItemsCarrito { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }
    }
}
