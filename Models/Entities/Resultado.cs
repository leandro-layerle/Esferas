namespace Esferas.Models.Entities
{
    public class Resultado
    {
        public int Id { get; set; }

        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public double Promedio { get; set; }
        public string Color { get; set; } // Rojo / Amarillo / Verde
    }
}