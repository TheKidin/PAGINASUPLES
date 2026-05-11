using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PIA.Data;
using PIA.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration; // 🛡️ Necesario para leer la bóveda secreta
using System; // Necesario para calcular fechas y horas
using System.Linq;
using PIA.Services; // 🛡️ Para el servicio de correos

namespace PIA.Controllers
{
    [Authorize] // Solo clientes autenticados pueden operar el arsenal
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config; // 🛡️ Escáner de configuración
        private readonly EmailSenderService _emailSender; // 🛡️ Servicio de correo

        public CarritoController(ApplicationDbContext context, IConfiguration config, EmailSenderService emailSender)
        {
            _context = context;
            _config = config;
            _emailSender = emailSender;
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
                if (operacion == 1) item.Cantidad += 1;
                else if (operacion == -1)
                {
                    item.Cantidad -= 1;
                    if (item.Cantidad <= 0) _context.ItemsCarrito.Remove(item);
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
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            var listaDeProductosStripe = new List<SessionLineItemOptions>();

            foreach (var item in miCarrito)
            {
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
                            Images = new List<string> { rutaImagen }
                        },
                    },
                    Quantity = item.Cantidad,
                });
            }

            if (!listaDeProductosStripe.Any()) return BadRequest(new { error = "Tu carrito está vacío." });

            var dominio = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = listaDeProductosStripe,
                Mode = "payment",
                SuccessUrl = dominio + "/Carrito/PagoExitoso?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = dominio + "/Carrito/Index",
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "MX" },
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);
            return Json(new { id = session.Id });
        }

        // ==========================================
        // 9. ACCIÓN: PROCESAR EL PAGO EXITOSO Y CREAR PEDIDO
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PagoExitoso(string session_id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var miCarrito = await _context.ItemsCarrito
                .Include(i => i.Variante)
                .ThenInclude(v => v.Producto)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();

            if (!miCarrito.Any()) return RedirectToAction("Index", "Home");

            var service = new SessionService();
            Session stripeSession = service.Get(session_id);

            string direccionEntrega = "Dirección no proporcionada";
            string ciudadEntrega = "No especificada";

            if (stripeSession.CustomerDetails?.Address != null)
            {
                var address = stripeSession.CustomerDetails.Address;
                direccionEntrega = $"{address.Line1} {address.Line2}, CP {address.PostalCode}".Trim();
                ciudadEntrega = $"{address.City}, {address.State}, {address.Country}".Trim();
            }

            // ⏱️ LÓGICA DE ENVÍOS (MTY ANTES DE LA 1 PM)
            DateTime fechaCalculada = DateTime.Now.AddDays(3);
            string ubicacionUpper = ciudadEntrega.ToUpper();

            bool esZonaMetro = ubicacionUpper.Contains("MONTERREY") ||
                               ubicacionUpper.Contains("N.L.") ||
                               ubicacionUpper.Contains("NL") ||
                               ubicacionUpper.Contains("APODACA") ||
                               ubicacionUpper.Contains("GUADALUPE") ||
                               ubicacionUpper.Contains("SAN NICOLÁS");

            if (esZonaMetro && DateTime.Now.Hour < 13) fechaCalculada = DateTime.Now;

            // 3. CREAR EL PEDIDO
            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId,
                FechaCompra = DateTime.Now,
                Total = miCarrito.Sum(i => (i.Variante?.Producto?.Precio ?? 0) * i.Cantidad),
                Direccion = direccionEntrega,
                Ciudad = ciudadEntrega,
                Estatus = "Pagado y Preparando Arsenal",
                FechaEntregaEstimada = fechaCalculada
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            // 4. DETALLES Y STOCK
            foreach (var item in miCarrito)
            {
                var detalle = new DetallePedido
                {
                    PedidoId = nuevoPedido.Id,
                    VarianteProductoId = item.Variante.Id,
                    Cantidad = item.Cantidad,
                    Precio = item.Variante?.Producto?.Precio ?? 0
                };
                _context.DetallesPedido.Add(detalle);

                if (item.Variante != null)
                {
                    item.Variante.Stock -= item.Cantidad;
                    if (item.Variante.Stock < 0) item.Variante.Stock = 0;
                }
            }

            // 5. VACIAR CARRITO
            _context.ItemsCarrito.RemoveRange(miCarrito);

            // 6. GUARDAR CAMBIOS FINALES
            await _context.SaveChangesAsync();

            // 📨 7. LANZAR TRANSMISIÓN AL SOLDADO (EMAIL)
            string correoCliente = User.Identity?.Name ?? "";

            if (!string.IsNullOrEmpty(correoCliente))
            {
                string asunto = $"Misión Confirmada: Orden #{nuevoPedido.Id.ToString("D4")} - SMUANL Performance";
                string html = $@"
                    <div style='background-color:#050505; color:#ffffff; font-family:Arial, sans-serif; padding:30px; border: 2px solid #222;'>
                        <h2 style='color:#dc3545; font-style:italic; letter-spacing:2px;'>SMUANL PERFORMANCE</h2>
                        <h3 style='border-bottom: 1px solid #333; padding-bottom: 10px;'>REPORTE DE OPERACIÓN LOGÍSTICA</h3>
                        <p>Soldado, tu despliegue de armamento ha sido autorizado y está siendo procesado.</p>
                        
                        <p><strong>NÚMERO DE ORDEN:</strong> #{nuevoPedido.Id.ToString("D4")}</p>
                        <p><strong>TOTAL ABONADO:</strong> ${nuevoPedido.Total.ToString("N2")} MXN</p>
                        <p><strong>COORDENADAS DE ATERRIZAJE:</strong> {nuevoPedido.Direccion}, {nuevoPedido.Ciudad}</p>
                        <p><strong>FECHA ESTIMADA DE IMPACTO:</strong> {nuevoPedido.FechaEntregaEstimada.ToString("dd MMM, yyyy")}</p>
                        
                        <p style='margin-top:20px; font-size:12px; color:#aaa;'>Mantente alerta a los radares para futuras actualizaciones del estatus.</p>
                    </div>";

                // Enviamos en segundo plano para no demorar la respuesta
                _ = _emailSender.EnviarCorreoAsync(correoCliente, asunto, html);
            }

            // 8. REDIRECCIÓN FINAL
            return RedirectToAction("Index", "Pedidos");
        }
    }
}