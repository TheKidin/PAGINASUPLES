using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIA.Data;
using Microsoft.AspNetCore.Authorization;
using PIA.Models;

namespace PIA.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            // Opcional: Traemos también las variantes para que no marque error si intentas contarlas en el Index
            return View(await _context.Productos.Include(p => p.Variantes).ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Variantes) // Añadimos esto para ver los sabores en los detalles del admin
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECCIÓN: Quitamos Sabor y Stock del Bind. Agregamos ImagenUrl por si la usas.
        public async Task<IActionResult> Create([Bind("Id,Nombre,Marca,Precio,ImagenUrl")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECCIÓN: Quitamos Sabor y Stock del Bind.
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Marca,Precio,ImagenUrl")] Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        // ==========================================
        // NUEVOS MÉTODOS PARA ADMINISTRAR SABORES
        // ==========================================

        public async Task<IActionResult> Sabores(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Variantes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarSabor(int productoId, string sabor, int stock)
        {
            var nuevaVariante = new VarianteProducto
            {
                ProductoId = productoId,
                Sabor = sabor,
                Stock = stock
            };

            _context.VariantesProducto.Add(nuevaVariante);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Sabores), new { id = productoId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSabor(int id, int productoId)
        {
            // Buscamos el sabor exacto que queremos borrar
            var sabor = await _context.VariantesProducto.FindAsync(id);

            if (sabor != null)
            {
                // Lo eliminamos de la base de datos
                _context.VariantesProducto.Remove(sabor);
                await _context.SaveChangesAsync();
            }

            // Recargamos la pantalla de sabores de este producto
            return RedirectToAction(nameof(Sabores), new { id = productoId });
        }
        // ==========================================
        // PANTALLA: EDITAR UN SABOR EXISTENTE
        // ==========================================
        public async Task<IActionResult> EditarSabor(int? id)
        {
            if (id == null) return NotFound();

            // Buscamos el sabor y de paso traemos los datos de su producto padre
            var variante = await _context.VariantesProducto
                .Include(v => v.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variante == null) return NotFound();

            return View(variante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSabor(int id, [Bind("Id,ProductoId,Sabor,Stock")] VarianteProducto variante)
        {
            if (id != variante.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(variante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.VariantesProducto.Any(e => e.Id == variante.Id)) return NotFound();
                    else throw;
                }
                // Si todo sale bien, lo regresamos a la tabla de sabores de ese producto
                return RedirectToAction(nameof(Sabores), new { id = variante.ProductoId });
            }
            return View(variante);
        }
    }
}