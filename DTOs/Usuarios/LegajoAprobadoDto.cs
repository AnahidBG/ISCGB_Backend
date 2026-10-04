namespace AutoGestionAPI.DTOs
{
    public class LegajoAprobadoDto
    {
        public int IdLegajo { get; set; }

        public string NombreUsuario { get; set; } = null!;

        public string TipoDocumento { get; set; } = null!;

        public string? RutaArchivo { get; set; }

        public DateTime FechaCarga { get; set; }

        public bool? PresentadoFisico { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public string? Comentario { get; set; }

        public string? Auditor { get; set; }
    }
}