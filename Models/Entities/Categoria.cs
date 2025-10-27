using Esferas.Models.Enums;

namespace Esferas.Models.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoCategoria Tipo { get; set; }

        public int? CategoriaPadreId { get; set; }
        public Categoria CategoriaPadre { get; set; }

        public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
    }
}