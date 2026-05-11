using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // 🛡️ BLOQUEO TÁCTICO: Solo usuarios con sesión iniciada
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
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🚀 OPTIMIZACIÓN: AsNoTracking mejora la velocidad de lectura
            var misPedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Variante)
                        .ThenInclude(v => v.Producto)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaCompra)
                .ToListAsync();

            return View(misPedidos);
        }

        // ==========================================
        // 2. VER DETALLES DE UNA MISIÓN ESPECÍFICA
        // ==========================================
        public async Task<IActionResult> Detalles(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Variante)
                        .ThenInclude(v => v.Producto)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            if (pedido == null) return NotFound("Misión no encontrada o acceso denegado.");

            return View(pedido);
        }

        // ==========================================
        // 3. PANEL MAESTRO: SOLO PARA EL COMANDANTE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> AdminIndex()
        {
            // 🛡️ ESCÁNER DE RANGO OFICIAL: Solo el rango "Admin" puede pasar
            if (!User.IsInRole("Admin")) return RedirectToAction("Index", "Home");

            var todosLosPedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Variante)
                        .ThenInclude(v => v.Producto)
                .OrderByDescending(p => p.FechaCompra)
                .ToListAsync();

            return View(todosLosPedidos);
        }

        // ==========================================
        // 4. ACCIÓN: CAMBIAR STATUS DE OPERACIÓN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ActualizarEstatus(int id, string nuevoEstatus)
        {
            // 🛡️ ESCÁNER DE RANGO OFICIAL
            if (!User.IsInRole("Admin")) return Forbid();

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                // 🔒 PROTOCOLO DE CANDADO: Si ya está cerrado, abortar el cambio
                if (pedido.Estatus == "Entregado" || pedido.Estatus == "Cancelado")
                {
                    return RedirectToAction("AdminIndex");
                }

                pedido.Estatus = nuevoEstatus;

                if (nuevoEstatus == "Entregado")
                {
                    pedido.FechaEntregaEstimada = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminIndex");
        }
    }
}