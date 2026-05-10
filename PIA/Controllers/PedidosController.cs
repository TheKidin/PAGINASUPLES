using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA.Data;
using PIA.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PIA.Controllers
{
    [Authorize] // 🛡️ BLOQUEO TÁCTICO: Solo usuarios con sesión iniciada pueden entrar aquí
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LA ZONA DE EXTRACCIÓN (CHECKOUT)
        // ==========================================
        public async Task<IActionResult> Checkout()
        {
            // 1. Obtenemos el ID del usuario que está conectado actualmente de forma segura
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Buscamos en la base de datos todo lo que tenga en su carrito
            var carritoDelUsuario = await _context.ItemsCarrito
                .Include(i => i.Variante)
                    .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == userId)
                .ToListAsync();

            // 3. Le mandamos esa lista real de productos a la Vista
            return View(carritoDelUsuario);
        }

        // ==========================================
        // 1. VER LISTA DE MIS PEDIDOS (Para Index.cshtml)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var misPedidos = await _context.Pedidos
                                           .Where(p => p.UsuarioId == usuarioId)
                                           .OrderByDescending(p => p.Fecha)
                                           .ToListAsync();

            return View(misPedidos);
        }

        // ==========================================
        // 2. VER DETALLES DE UNA MISIÓN ESPECÍFICA (Para Detalles.cshtml)
        // ==========================================
        public async Task<IActionResult> Detalles(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ⚠️ CORRECCIÓN TÁCTICA: "Detalles" en plural
            var pedido = await _context.Pedidos
                                       .Include(p => p.Detalles) // Carga la lista de lo que compró
                                          .ThenInclude(d => d.Variante) // Carga el Sabor
                                             .ThenInclude(v => v.Producto) // Carga la Foto y Nombre
                                       .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            if (pedido == null)
            {
                return NotFound("Misión no encontrada o acceso denegado.");
            }

            return View(pedido);
        }

        // ==========================================
        // FASE 2: PROCESAR EL PAGO EXITOSO EN LA BD
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ProcesarCompraExitosa(string transaccionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Traer el carrito actual
            var carrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == userId)
                .ToListAsync();

            if (!carrito.Any()) return BadRequest(new { success = false, message = "El carrito está vacío" });

            // 2. Calcular total
            decimal totalFinal = carrito.Sum(i => (i.Variante?.Producto?.Precio ?? 0) * i.Cantidad);

            // 3. Crear el Registro del Pedido
            var nuevoPedido = new Pedido
            {
                UsuarioId = userId,
                Fecha = DateTime.Now,
                Total = totalFinal,
                Estado = "Pagado",

                // ⚠️ CORRECCIÓN TÁCTICA: "Detalles" en plural
                Detalles = carrito.Select(item => new DetallePedido
                {
                    VarianteProductoId = item.VarianteProductoId,
                    Cantidad = item.Cantidad,
                    Precio = item.Variante?.Producto?.Precio ?? 0
                }).ToList()
            };

            _context.Pedidos.Add(nuevoPedido);

            // 4. Restar el Stock de tu arsenal
            foreach (var item in carrito)
            {
                if (item.Variante != null)
                {
                    item.Variante.Stock -= item.Cantidad; // ¡Aquí funciona tu alerta de Stock Bajo!
                }
            }

            // 5. Vaciar el Carrito del usuario
            _context.ItemsCarrito.RemoveRange(carrito);

            // 6. Guardar todos los cambios de golpe en la Base de Datos
            await _context.SaveChangesAsync();

            // 7. Avisarle a la pantalla que todo salió perfecto y mandarle el ID del nuevo pedido
            return Json(new { success = true, orderId = nuevoPedido.Id });
        }

        // ==========================================
        // FASE 3: LA PANTALLA DE VICTORIA (ÉXITO)
        // ==========================================
        public async Task<IActionResult> Exito(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscamos el pedido para mostrarle el total y su número de orden
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == userId);

            // Si por alguna razón no existe, lo mandamos al inicio
            if (pedido == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(pedido);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmarCompra(string direccion, string ciudad)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var carrito = await _context.ItemCarritos
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Variante)
                    .ThenInclude(v => v.Producto)
                .ToListAsync();

            if (!carrito.Any()) return RedirectToAction("Index", "Carrito");

            // --- CÁLCULO TÁCTICO DE ENTREGA ---
            DateTime entrega = DateTime.Now.AddDays(2); // Por defecto 48hrs
            DateTime ahora = DateTime.Now;

            // REGLA MTY: Si es Monterrey y antes de la 1:00 PM (13:00 hrs)
            if (ciudad.ToLower().Contains("monterrey") || ciudad.ToLower().Contains("mty"))
            {
                if (ahora.Hour < 13)
                {
                    entrega = ahora; // ⚡ ENVÍO HOY MISMO
                }
                else
                {
                    entrega = ahora.AddDays(1); // Mañana
                }
            }

            // --- CREAR EL REGISTRO DE OPERACIÓN ---
            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId!,
                FechaCompra = ahora,
                Total = carrito.Sum(x => (x.Variante?.Producto?.Precio ?? 0) * x.Cantidad),
                Direccion = direccion,
                Ciudad = ciudad,
                Estatus = "En Preparación",
                FechaEntregaEstimada = entrega
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            // --- PASAR LOS PRODUCTOS AL HISTORIAL ---
            foreach (var item in carrito)
            {
                var detalle = new DetallePedido
                {
                    PedidoId = nuevoPedido.Id,
                    VarianteProductoId = item.VarianteId,
                    Cantidad = item.Cantidad,
                    Precio = item.Variante?.Producto?.Precio ?? 0
                };
                _context.DetallesPedido.Add(detalle);
            }

            // Limpiar el carrito del soldado
            _context.ItemCarritos.RemoveRange(carrito);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmacion", new { id = nuevoPedido.Id });
        }
    }
}