using Microsoft.AspNetCore.Mvc;
using PIA.Models;
using System.Diagnostics;

namespace PIA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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

        public IActionResult Proteinas()
        {
            ViewData["Title"] = "Proteínas";
            return View();
        }

        public IActionResult DetalleProducto(string nombre)
        {
            ViewBag.NombreProducto = nombre;
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
