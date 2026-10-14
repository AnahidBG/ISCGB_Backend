namespace AutoGestionAPI.DTOs.Legajos
{
    public class DocumentoFaltanteDto
    {
        public int IdTipoDocumento { get; set; }
        public required string NombreDocumento { get; set; }
        public bool EsObligatorio { get; set; }
    }
}