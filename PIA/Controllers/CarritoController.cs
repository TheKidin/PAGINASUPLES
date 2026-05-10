using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PIA.Data;
using PIA.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

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
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                var nuevoItem = new ItemCarrito
                {
                    VarianteProductoId = varianteId,
                    Cantidad = cantidad,
                    UsuarioId = usuarioId
                };
                _context.ItemsCarrito.Add(nuevoItem);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Producto sincronizado con el arsenal lateral." });
        }

        // ==========================================
        // 3. VISTA PARCIAL: LISTA DEL MENÚ LATERAL
        // ==========================================
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
        // 5. ACCIÓN: ELIMINAR DEL ARSENAL
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.ItemsCarrito.FindAsync(id);
            if (item != null)
            {
                _context.ItemsCarrito.Remove(item);
                await _context.SaveChangesAsync();
            }
            // En vez de devolver JSON, recargamos la página del carrito
            return RedirectToAction("Index");
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
                if (operacion == 1)
                {
                    item.Cantidad += 1;
                }
                else if (operacion == -1)
                {
                    item.Cantidad -= 1;
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
            if (string.IsNullOrEmpty(usuarioId)) return Json(0);

            var totalBotes = await _context.ItemsCarrito
                .Where(i => i.UsuarioId == usuarioId)
                .SumAsync(i => i.Cantidad);

            return Json(totalBotes);
        }

        // ==========================================
        // 8. ACCIÓN AJAX: CREAR SESIÓN DE PAGO STRIPE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CrearSesionStripe()
        {
            // 1. INYECTAR LA LLAVE SECRETA
            StripeConfiguration.ApiKey = "sk_test_51TSsHcLrhJpcq2a9U0WvpxlJwvlCXpRPRr3PTBCwKua1u5p2UxuI7oQJ1afV5i2OwFkKCCU5NS4dVLi28JhlDDGx00xXfbKQeU";

            // 2. IDENTIFICAR AL SOLDADO Y TRAER SU ARSENAL
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            // 3. CREAR LA LISTA DETALLADA PARA STRIPE CON IMÁGENES
            var listaDeProductosStripe = new List<SessionLineItemOptions>();

            foreach (var item in miCarrito)
            {
              // URL imagen para qu aparezca en STRIPE
                var rutaImagen = item.Variante?.Producto?.ImagenUrl ?? "https://dummyimage.com/200x200/cccccc/000000.png&text=Sin+Imagen";

                listaDeProductosStripe.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)((item.Variante?.Producto?.Precio ?? 0) * 100),
                        Currency = "mxn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Variante?.Producto?.Nombre ?? "Producto Desconocido",
                            Description = "Sabor: " + (item.Variante?.Sabor ?? "N/A"),

                            //  Aquí se inyecta la URL pública de la imagen
                            Images = new List<string> { rutaImagen }
                        },
                    },
                    Quantity = item.Cantidad,
                });
            }

            // Validar que no disparen con el carrito vacío
            if (!listaDeProductosStripe.Any())
            {
                return BadRequest(new { error = "Tu carrito está vacío, Comandante." });
            }

            // Obtenemos la URL exacta de tu radar dinámicamente para que no haya fallos
            var dominio = $"{Request.Scheme}://{Request.Host}";

            // 4. CONFIGURAR LA BÓVEDA CON LA LISTA DETALLADA
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = listaDeProductosStripe,
                Mode = "payment",
                SuccessUrl = dominio + "/Carrito/PagoExitoso", // <--- Lo manda a la nueva zona de aterrizaje
                CancelUrl = dominio + "/Carrito/Index", // <--- Si cancela, vuelve al carrito
            };

            var service = new SessionService();
            Session session = service.Create(options);

            // 5. RETORNAR EL GAFETE DE ACCESO A LA VISTA
            return Json(new { id = session.Id });
        }

        // ==========================================
        // 9. ACCIÓN: PROCESAR EL PAGO EXITOSO
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PagoExitoso()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Traer los productos que el soldado acaba de pagar
            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            if (miCarrito.Any())
            {
                // 2. DESCONTAR DEL INVENTARIO
                foreach (var item in miCarrito)
                {
                    if (item.Variante != null)
                    {
                      
                        item.Variante.Stock -= item.Cantidad;

                        // Opcional: Asegurarnos de que el stock no baje de 0
                        if (item.Variante.Stock < 0) item.Variante.Stock = 0;
                    }
                }

                // 3. VACIAR EL CARRITO (Limpiar el arsenal)
                _context.ItemsCarrito.RemoveRange(miCarrito);

                // 4. GUARDAR TODOS LOS CAMBIOS EN LA BASE DE DATOS
                await _context.SaveChangesAsync();
            }

            // 5. Misión cumplida: Lo mandamos a la pantalla de inicio sano y salvo
            return RedirectToAction("Index", "Home");
        }
    }
}