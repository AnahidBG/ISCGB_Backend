namespace AutoGestionAPI.DTOs
{
    public class JustificativoResponseDto
    {
        public int IdJustificativo { get; set; }
        public string? TipoInasistencia { get; set; }
        public string? RutaArchivo { get; set; }
        public string? NotaAdicional { get; set; }
        public DateTime FechaCarga { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaInasistenciaInicio { get; set; }
        public DateTime? FechaInasistenciaFin { get; set; }
        public int? IdUsuarioAuditor { get; set; }
    }
}