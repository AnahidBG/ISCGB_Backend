namespace AutoGestionAPI.DTOs
{
    public class ResultadoBusquedaDto
    {
        public string Tipo { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string? Subtitulo { get; set; }
        public int IdReferencia { get; set; }
    }
}