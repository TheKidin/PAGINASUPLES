namespace PIA.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }

        // Mágia relacional: Un producto puede tener MUCHOS sabores
        public List<VarianteProducto> Variantes { get; set; } = new List<VarianteProducto>();
    }
}