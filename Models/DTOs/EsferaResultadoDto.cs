using Esferas.Models.Enums;

namespace Esferas.Models.DTOs
{
    public class EsferaResultadoDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public double Promedio { get; set; }
        public string ColorSemaforo { get; set; } // "#28a745", "#ffc107", "#dc3545"
    }
}
