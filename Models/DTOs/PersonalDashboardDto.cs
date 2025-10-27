namespace Esferas.Models.DTOs
{
    public class PersonalDashboardDto
    {
        public string NombrePersona { get; set; }
        public string NombreEncuesta { get; set; }
        public DateTime Fecha { get; set; }

        public double PromedioGeneral { get; set; }
        public int CantRojas { get; set; }
        public int CantAmarillas { get; set; }
        public int CantVerdes { get; set; }

        public List<(string Esfera, double Promedio)> RadarEsferas { get; set; } = new();

        // Listas por color (solo nombres y promedio; texto/desc opcional)
        public List<(string Esfera, double Promedio, string? BreveDescripcion)> EsferasRojas { get; set; } = new();
        public List<(string Esfera, double Promedio, string? BreveDescripcion)> EsferasAmarillas { get; set; } = new();
        public List<(string Esfera, double Promedio, string? BreveDescripcion)> EsferasVerdes { get; set; } = new();

        // Recomendaciones (curso/lectura/acción) — opcional
        public List<RecomendacionDto> Recomendaciones { get; set; } = new();
    }
}
