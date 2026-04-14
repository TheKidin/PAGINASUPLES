using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PIA.Data;
using PIA.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PIA.Controllers
{
    [Authorize] // Solo clientes que hayan iniciado sesión pueden usar el carrito
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ACCIÓN: AGREGAR AL CARRITO
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, int varianteId)
        {
            // 1. Identificar quién es el cliente que está comprando
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Revisar si ese sabor exacto ya está en su carrito
            var itemExistente = await _context.ItemsCarrito
                .FirstOrDefaultAsync(i => i.VarianteProductoId == varianteId && i.UsuarioId == usuarioId);

            if (itemExistente != null)
            {
                // Si ya lo tiene, solo le sumamos 1 a la cantidad de botes
                itemExistente.Cantidad += 1;
            }
            else
            {
                // Si es un sabor nuevo para él, lo agregamos a su caja
                var nuevoItem = new ItemCarrito
                {
                    VarianteProductoId = varianteId,
                    Cantidad = 1,
                    UsuarioId = usuarioId
                };
                _context.ItemsCarrito.Add(nuevoItem);
            }

            // 3. Guardar el movimiento en la base de datos
            await _context.SaveChangesAsync();

            // 4. Mandarlo a la pantalla de "Mi Carrito"
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // PANTALLA: VER MI CARRITO (En construcción)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Por ahora solo leemos los productos de este cliente y abrimos la vista
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto) // Traemos la info de la variante y del bote principal
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            return View("~/Views/Carrito/Index.cshtml", miCarrito);
        }
    }
}