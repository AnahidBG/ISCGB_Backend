namespace AutoGestionAPI.DTOs
{
    public class LegajoDetalleDto
    {
        public int IdLegajo { get; set; }
        public int IdUsuario { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string RutaArchivo { get; set; } = null!;
        public DateTime FechaCarga { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; } = null!;
        public bool? PresentadoFisico { get; set; }
        public string? Comentario { get; set; }
        public string Auditor { get; set; } = null!;
    }
}