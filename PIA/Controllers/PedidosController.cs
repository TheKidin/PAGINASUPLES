using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PIA.Data; // <-- Asegúrate de que este sea el nombre de tu proyecto .Data
using PIA.Models; // <-- Asegúrate de que este sea el nombre de tu proyecto .Models
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace PIA.Controllers // <-- Si tu proyecto no se llama PIA, cambia esto
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
        // 1. VER LISTA DE MIS PEDIDOS (Para Index.cshtml)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Identificar el ID del usuario que está logueado en este momento
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ir a la base de datos y traer SOLO los pedidos de este usuario, del más nuevo al más viejo
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

            // Buscar el pedido exacto por su ID, asegurando que le pertenezca a este usuario (Seguridad)
            var pedido = await _context.Pedidos
                                       // .Include(p => p.Detalles) // Descomenta esto si tienes una tabla de Detalles conectada
                                       // .ThenInclude(d => d.Producto)
                                       .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            if (pedido == null)
            {
                // Si alguien intenta poner un ID de pedido que no es suyo en la URL, lo bloqueamos
                return NotFound("Misión no encontrada o acceso denegado.");
            }

            return View(pedido);
        }
    }
}