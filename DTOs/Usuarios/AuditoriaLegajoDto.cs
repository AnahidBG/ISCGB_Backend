namespace AutoGestionAPI.DTOs
{
    public class AuditoriaLegajoDto
    {
        public string Estado { get; set; } = null!;
        public string? Comentario { get; set; }
        public bool? PresentadoFisico { get; set; }
    }
}