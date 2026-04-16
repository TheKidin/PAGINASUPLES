using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PIA.Data;
using PIA.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PIA.Controllers
{
    [Authorize] // Solo clientes autenticados pueden operar el arsenal
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. PANTALLA COMPLETA: VER MI CARRITO
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            return View("~/Views/Carrito/Index.cshtml", miCarrito);
        }

        // ==========================================
        // 2. ACCIÓN AJAX: AGREGAR AL ARSENAL
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, int varianteId, int cantidad = 1)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var itemExistente = await _context.ItemsCarrito
                .FirstOrDefaultAsync(i => i.VarianteProductoId == varianteId && i.UsuarioId == usuarioId);

            if (itemExistente != null)
            {
                // Si ya lo tiene, le sumamos la cantidad
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                // Si es nuevo, lo agregamos
                var nuevoItem = new ItemCarrito
                {
                    VarianteProductoId = varianteId,
                    Cantidad = cantidad,
                    UsuarioId = usuarioId
                };
                _context.ItemsCarrito.Add(nuevoItem);
            }

            await _context.SaveChangesAsync();

            // Devolvemos JSON para que el JavaScript abra el Offcanvas (Menú Lateral)
            return Json(new { success = true, message = "Producto sincronizado con el arsenal lateral." });
        }

        // ==========================================
        // 3. VISTA PARCIAL: LISTA DEL MENÚ LATERAL
        // ==========================================
        // Este método devuelve solo el HTML de los productos para el Offcanvas
        public async Task<IActionResult> ObtenerMiniCarrito()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            return PartialView("_MiniCarrito", items);
        }

        // ==========================================
        // 4. VISTA PARCIAL: TOTALES DEL MENÚ LATERAL
        // ==========================================
        public async Task<IActionResult> ObtenerMiniCarritoFooter()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            return PartialView("_MiniCarritoFooter", items);
        }

        // ==========================================
        // 5. ACCIÓN AJAX: ELIMINAR DEL ARSENAL
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.ItemsCarrito.FindAsync(id);
            if (item != null)
            {
                // Corregido: Usamos ItemsCarrito en lugar de Carrito
                _context.ItemsCarrito.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "No se pudo encontrar el item." });
        }

        // ==========================================
        // 6. ACCIÓN AJAX: ACTUALIZAR CANTIDAD (+ / -)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int id, int operacion)
        {
            var item = await _context.ItemsCarrito.FindAsync(id);
            if (item != null)
            {
                if (operacion == 1) // Le dio al botón de '+'
                {
                    item.Cantidad += 1;
                }
                else if (operacion == -1) // Le dio al botón de '-'
                {
                    item.Cantidad -= 1;

                    // Si le resta hasta llegar a 0, lo eliminamos del arsenal
                    if (item.Cantidad <= 0)
                    {
                        _context.ItemsCarrito.Remove(item);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "No se pudo encontrar el armamento." });
        }
        // ==========================================
        // 7. ACCIÓN AJAX: OBTENER CONTADOR DEL NAVBAR
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ObtenerContador()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si el usuario no ha iniciado sesión, el carrito está en 0
            if (string.IsNullOrEmpty(usuarioId)) return Json(0);

            // Sumamos la cantidad total de botes
            var totalBotes = await _context.ItemsCarrito
                .Where(i => i.UsuarioId == usuarioId)
                .SumAsync(i => i.Cantidad);

            return Json(totalBotes);
        }
    }
}