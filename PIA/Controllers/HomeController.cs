using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIA.Data;
using PIA.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PIA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        #region Vistas Generales y Navegación

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                                          .OrderByDescending(p => p.Id)
                                          .Take(10)
                                          .ToListAsync();
            return View(productos);
        }

        public IActionResult Catalogo(string? categorias = null)
        {
            if (string.IsNullOrEmpty(categorias)) return View();

            ViewBag.CategoriaSeleccionada = categorias;
            return View("ProductosPorCategoria");
        }

        public IActionResult Categoria() => View();
        public IActionResult Contacto() => View();
        public IActionResult Nuevo() => View();

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                                         .Include(p => p.Variantes)
                                         .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        #endregion

        #region Catálogos de Armamento (Filtros Activos)

        // =========================================================
        // 1. CATÁLOGO DE PROTEÍNAS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Proteinas(List<string> tipos, string marca)
        {
            ViewData["Title"] = "Proteínas";

            // Filtro estricto: Bloqueamos todo lo que pertenezca a otras categorías
            var query = _context.Productos.Where(p =>
                !p.Nombre.ToLower().Contains("creatina") &&
                !p.Nombre.ToLower().Contains("creatine") &&
                !p.Nombre.ToLower().Contains("amino") &&
                !p.Nombre.ToLower().Contains("bcaa") &&
                !p.Nombre.ToLower().Contains("pre") &&
                !p.Nombre.ToLower().Contains("workout") &&
                !p.Nombre.ToLower().Contains("pump") &&
                !p.Nombre.ToLower().Contains("stim-free") &&
                !p.Nombre.ToLower().Contains("stim free") &&
                !p.Nombre.ToLower().Contains("evp") &&
                !p.Nombre.ToLower().Contains("creagumm") &&
                !p.Nombre.ToLower().Contains("creactor") &&
                !p.Nombre.ToLower().Contains("creakong") &&
                !p.Nombre.ToLower().Contains("c4 original") &&
                !p.Nombre.ToLower().Contains("madness") // ⚠️ Bloqueamos Madness de las proteínas
            );

            var proteinasBase = await query.ToListAsync();

            if (tipos != null && tipos.Any())
            {
                proteinasBase = proteinasBase.Where(p =>
                    (tipos.Contains("Whey Protein") && p.Nombre.Contains("Whey", StringComparison.OrdinalIgnoreCase)) ||
                    (tipos.Contains("Isolatada") && (p.Nombre.Contains("Isolate", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Isolatada", StringComparison.OrdinalIgnoreCase))) ||
                    (tipos.Contains("Vegana") && (p.Nombre.Contains("Vegan", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Plant", StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(marca))
            {
                proteinasBase = proteinasBase.Where(p => p.Marca == marca).ToList();
            }

            ViewBag.TiposSeleccionados = tipos ?? new List<string>();
            ViewBag.MarcaSeleccionada = marca;

            return View(proteinasBase);
        }

        // ==========================================
        // 2. CATÁLOGO DE CREATINAS
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Creatinas(List<string> tipos, string marca)
        {
            ViewData["Title"] = "Creatinas";

            var query = _context.Productos.Where(p =>
                p.Nombre.ToLower().Contains("creatina") ||
                p.Nombre.ToLower().Contains("creatine") ||
                p.Nombre.ToLower().Contains("creagumm") ||
                p.Nombre.ToLower().Contains("creactor") ||
                p.Nombre.ToLower().Contains("creakong")
            );

            var creatinasBase = await query.ToListAsync();

            if (!string.IsNullOrEmpty(marca))
            {
                creatinasBase = creatinasBase.Where(p => p.Marca == marca).ToList();
            }

            ViewBag.TiposSeleccionados = tipos ?? new List<string>();
            ViewBag.MarcaSeleccionada = marca;

            return View(creatinasBase);
        }

        // =========================================================
        // 3. CATÁLOGO DE AMINOÁCIDOS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Aminoacidos(List<string> tipos, string marca)
        {
            ViewData["Title"] = "Aminoácidos";

            var query = _context.Productos.Where(p =>
                p.Nombre.ToLower().Contains("amino") ||
                p.Nombre.ToLower().Contains("bcaa")
            );

            var aminosBase = await query.ToListAsync();

            if (tipos != null && tipos.Any())
            {
                aminosBase = aminosBase.Where(p =>
                    (tipos.Contains("BCAAs") && p.Nombre.Contains("BCAA", StringComparison.OrdinalIgnoreCase)) ||
                    (tipos.Contains("EAAs") && p.Nombre.Contains("EAA", StringComparison.OrdinalIgnoreCase)) ||
                    (tipos.Contains("Glutamina") && p.Nombre.Contains("Gluta", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(marca))
            {
                aminosBase = aminosBase.Where(p => p.Marca == marca).ToList();
            }

            ViewBag.TiposSeleccionados = tipos ?? new List<string>();
            ViewBag.MarcaSeleccionada = marca;

            return View(aminosBase);
        }

        // =========================================================
        // 4. CATÁLOGO DE PRE-WORKOUTS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> PreWorkouts(List<string> tipos, string marca)
        {
            ViewData["Title"] = "Pre-Workouts";

            // ⚠️ FIX: Añadimos "madness" al radar de Pre-Workouts
            var query = _context.Productos.Where(p =>
                p.Nombre.ToLower().Contains("pre") ||
                p.Nombre.ToLower().Contains("workout") ||
                p.Nombre.ToLower().Contains("pump") ||
                p.Nombre.ToLower().Contains("stim-free") ||
                p.Nombre.ToLower().Contains("stim free") ||
                p.Nombre.ToLower().Contains("evp") ||
                p.Nombre.ToLower().Contains("c4") ||
                p.Nombre.ToLower().Contains("madness") // <--- ¡Misión cumplida!
            );

            var preworkoutsBase = await query.ToListAsync();

            if (tipos != null && tipos.Any())
            {
                preworkoutsBase = preworkoutsBase.Where(p =>
                    (tipos.Contains("Con Estimulantes") && (!p.Nombre.Contains("Pump", StringComparison.OrdinalIgnoreCase) && !p.Nombre.Contains("Free", StringComparison.OrdinalIgnoreCase))) ||
                    (tipos.Contains("Pump (Sin Estimulantes)") && (p.Nombre.Contains("Pump", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Stim-Free", StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains("Stim Free", StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(marca))
            {
                preworkoutsBase = preworkoutsBase.Where(p => p.Marca == marca).ToList();
            }

            ViewBag.TiposSeleccionados = tipos ?? new List<string>();
            ViewBag.MarcaSeleccionada = marca;

            return View(preworkoutsBase);
        }

        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}