namespace AutoGestionAPI.DTOs
{
    public class LegajoPendienteDto
    {
        public int IdLegajo { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
        public string? RutaArchivo { get; set; } = null!;
        public DateTime FechaCarga { get; set; }
        public bool? PresentadoFisico { get; set; }
    }
}