namespace PIA.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public decimal Precio { get; set; }

        // ¡Metemos todo en la misma caja!
        public string Sabor { get; set; }
        public int Stock { get; set; }
    }
}