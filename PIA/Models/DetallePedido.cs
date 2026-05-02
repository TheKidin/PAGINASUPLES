using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA.Models
{
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        // 1. Conexión a la Orden Principal
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        // 2. Conexión al Producto Exacto (y su sabor)
        public int VarianteProductoId { get; set; }
        public VarianteProducto? Variante { get; set; }

        // 3. Datos de la compra en ese momento
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; } // El precio al que se le vendió en ese momento
    }
}