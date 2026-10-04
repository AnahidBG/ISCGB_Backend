namespace AutoGestionAPI.DTOs
{
    public class JustificativoGeneralDto
    {
        public int IdJustificativo { get; set; }
        public string NombreUsuario { get; set; } = null!; // Acá va a ir Nombre + Apellido
        public string? TipoInasistencia { get; set; }
        public string? Estado { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime? FechaInasistenciaInicio { get; set; }
        public DateTime? FechaInasistenciaFin { get; set; }
    }
}