namespace Esferas.Models.DTOs
{
    public class DashboardKpiDto
    {
        public int TotalEncuestas { get; set; }
        public int EncuestasCompletadas { get; set; }
        public double PromedioEfectividad { get; set; }
        public int EsferasEnRojo { get; set; }
    }
}
