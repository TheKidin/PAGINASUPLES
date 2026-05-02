using System;
using System.Collections.Generic; // ⚠️ Necesario para usar List<>
using System.ComponentModel.DataAnnotations;

namespace PIA.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; } // El número de orden táctica

        [Required]
        public string UsuarioId { get; set; } // Para saber de qué cliente es este pedido

        public DateTime Fecha { get; set; } // Cuándo se hizo el despliegue

        public string Estado { get; set; } // Ej. "Procesando", "En Camino", "Entregado"

        public decimal Total { get; set; } // El total invertido

        // ⚠️ LA MOCHILA TÁCTICA: Aquí se guardará la lista de todo lo que compró
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}