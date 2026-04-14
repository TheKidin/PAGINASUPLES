namespace PIA.Models
{
    public class VarianteProducto
    {
        public int Id { get; set; }

        // Esta es la "cuerda" que amarra el sabor al bote principal
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public string Sabor { get; set; } = string.Empty;

        // ¡El stock ahora le pertenece al sabor, no al bote genérico!
        public int Stock { get; set; }
    }
}