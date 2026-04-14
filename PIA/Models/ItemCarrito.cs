namespace PIA.Models
{
    public class ItemCarrito
    {
        public int Id { get; set; }

        // La conexión al sabor exacto que el cliente eligió
        public int VarianteProductoId { get; set; }
        public VarianteProducto? Variante { get; set; }

        // ¿Cuántos botes quiere llevarse?
        public int Cantidad { get; set; }

        // El ID del cliente (para que tu carrito no se mezcle con el mío)
        public string UsuarioId { get; set; } = string.Empty;
    }
}