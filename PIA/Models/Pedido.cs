using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIA.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public decimal Total { get; set; }

        // Datos de Entrega
        [Required(ErrorMessage = "La dirección es obligatoria para el despliegue")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es necesaria para calcular la logística")]
        public string Ciudad { get; set; } = string.Empty;

        public string Estatus { get; set; } = "Preparando Arsenal"; // Estatus inicial
        public DateTime FechaEntregaEstimada { get; set; }

        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}