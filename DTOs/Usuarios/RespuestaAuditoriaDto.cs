namespace AutoGestionAPI.DTOs
{
    public class RespuestaAuditoriaDto
    {
        public string Message { get; set; } = null!;
        public int IdLegajo { get; set; }
        public string Estado { get; set; } = null!;
    }
}