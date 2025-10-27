using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Esferas.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Esferas.Models.ViewModels
{
    public class EncuestaViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        [Display(Name = "Empresa")]
        public int EmpresaId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required]
        public EstadoEncuesta Estado { get; set; }

        [ValidateNever]
        public SelectList EmpresasDisponibles { get; set; }
    }
}
