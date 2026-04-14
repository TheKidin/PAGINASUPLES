namespace PIA.Models
{
    public class ItemCarrito
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int Cantidad { get; set; }

        // Lo cambiamos para que apunte directo al Producto (que ya tiene el sabor adentro)
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}