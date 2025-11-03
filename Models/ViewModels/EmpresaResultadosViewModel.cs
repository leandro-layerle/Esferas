using Esferas.Models.DTOs;
using Esferas.Models.Enums;

namespace Esferas.Models.ViewModels
{
    public class EmpresaResultadosViewModel
    {
        public double PromedioGeneral { get; set; }
        public string ColorPromedioGeneral { get; set; } // "#28a745", "#ffc107", "#dc3545"
        public List<EsferaResultadoDto> EsferasPrimarias { get; set; } = new();
    }
}
