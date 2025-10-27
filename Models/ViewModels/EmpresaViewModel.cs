using System.ComponentModel.DataAnnotations;

namespace Esferas.Models.ViewModels
{
    public class EmpresaViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Display(Name = "Cultura organizacional")]
        [StringLength(500)]
        public string Cultura { get; set; }
    }
}
