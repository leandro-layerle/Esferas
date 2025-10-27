namespace Esferas.Models.DTOs
{
    public class RecomendacionDto
    {
        public string Esfera { get; set; }
        public string Tipo { get; set; } // "Curso", "Lectura", "Acción"
        public string Titulo { get; set; }
        public string? Url { get; set; }
        public string? Notas { get; set; }
    }
}
