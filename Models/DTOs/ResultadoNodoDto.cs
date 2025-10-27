namespace Esferas.Models.DTOs
{
    public class ResultadoNodoDto
    {
        public string Name { get; set; }
        public double Promedio { get; set; }
        public string Color { get; set; }
        public List<ResultadoNodoDto> Children { get; set; } = new();
    }
}
