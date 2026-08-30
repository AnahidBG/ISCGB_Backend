namespace AutoGestionAPI.DTOs
{
    // Este sub-DTO representa cada archivo individual de forma liviana
    public class DocumentoSubidoDto
    {
        public int IdLegajo { get; set; }
        public int? IdTipoDoc { get; set; }
        public string? Estado { get; set; }
        public string? RutaArchivo { get; set; }
    }

    public class ResumenLegajoDto
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Dni { get; set; } = null!;

        // Acá mandamos la lista completa de documentos
        public List<DocumentoSubidoDto> Documentos { get; set; } = new List<DocumentoSubidoDto>();
    }
}