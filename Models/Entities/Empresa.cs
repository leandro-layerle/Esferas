namespace Esferas.Models.Entities
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Cultura { get; set; }

        public ICollection<Encuesta> Encuestas { get; set; }
    }
}