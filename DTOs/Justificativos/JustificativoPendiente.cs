namespace AutoGestionAPI.DTOs
{
    public class JustificativoPendienteDto
    {
        public int IdJustificativo { get; set; }
        public string NombreDocente { get; set; } = null!;
        public string TipoInasistencia { get; set; } = null!;
        public string? RutaArchivo { get; set; }
        public DateTime FechaCarga { get; set; }
    }
}