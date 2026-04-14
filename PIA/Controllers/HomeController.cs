using Microsoft.AspNetCore.Mvc;
using PIA.Models;
using System.Diagnostics;
using PIA.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq; // Nos permite usar los filtros como .Where y .OrderBy

namespace PIA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Nuestra conexión a la BD

        // Constructor
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==========================================
        // PÁGINA DE INICIO
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Vamos a la base de datos y traemos los últimos 4 productos registrados
            var productos = await _context.Productos
                                          .OrderByDescending(p => p.Id)
                                          .Take(4)
                                          .ToListAsync();

            return View(productos);
        }

        public IActionResult Catalogo(string categorias = null)
        {
            if (string.IsNullOrEmpty(categorias))
            {
                return View();
            }

            ViewBag.CategoriaSeleccionada = categorias;
            return View("ProductosPorCategoria");
        }

        public IActionResult Categoria()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }

        public IActionResult Nuevo()
        {
            return View();
        }

        // ========================================================
        // PANTALLA GIGANTE: Mostrar el detalle de un suplemento
        // ========================================================
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Agregamos el Include para traernos la lista de sabores
            var producto = await _context.Productos
                                         .Include(p => p.Variantes)
                                         .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // ==========================================
        // CATÁLOGO DE PROTEÍNAS
        // ==========================================
        public async Task<IActionResult> Proteinas()
        {
            ViewData["Title"] = "Proteínas";

            // EL FILTRO SÚPER ESTRICTO: Bloquea creatinas, aminos y bcaas
            var proteinas = await _context.Productos
                .Where(p => !p.Nombre.ToLower().Contains("creatina")
                         && !p.Nombre.ToLower().Contains("creatine")
                         && !p.Nombre.ToLower().Contains("amino")
                         && !p.Nombre.ToLower().Contains("bcaa")
                         && !p.Nombre.ToLower().Contains("pre")
                         && !p.Nombre.ToLower().Contains("workout"))
                .ToListAsync();

            return View(proteinas);
        }

        // ==========================================
        // CATÁLOGO DE CREATINAS
        // ==========================================
        public async Task<IActionResult> Creatinas()
        {
            ViewData["Title"] = "Creatinas";

            // EL FILTRO: Traer SOLO lo que diga creatina (sin importar mayúsculas)
            var creatinas = await _context.Productos
                .Where(p => p.Nombre.ToLower().Contains("creatina") || p.Nombre.ToLower().Contains("creatine"))
                .ToListAsync();

            return View(creatinas);
        }

        // ==========================================
        // CATÁLOGO DE AMINOÁCIDOS
        // ==========================================
        public async Task<IActionResult> Aminoacidos()
        {
            ViewData["Title"] = "Aminoácidos";

            // EL FILTRO: Traer SOLO lo que diga amino o bcaa (sin importar mayúsculas)
            var aminoacidos = await _context.Productos
                .Where(p => p.Nombre.ToLower().Contains("amino") || p.Nombre.ToLower().Contains("bcaa"))
                .ToListAsync();

            return View(aminoacidos);
        }

        // ==========================================
        // CATÁLOGO DE PRE-WORKOUTS
        // ==========================================
        public async Task<IActionResult> PreWorkouts()
        {
            ViewData["Title"] = "Pre-Workouts";

            // EL FILTRO: Traer SOLO lo que diga "pre" o "workout"
            var preworkouts = await _context.Productos
                .Where(p => p.Nombre.ToLower().Contains("pre") || p.Nombre.ToLower().Contains("workout"))
                .ToListAsync();

            return View(preworkouts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}