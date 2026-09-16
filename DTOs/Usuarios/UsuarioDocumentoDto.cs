using System.Collections.Generic;

namespace AutoGestionAPI.DTOs
{
    public class UsuarioDocumentosDto
    {
        public string NombreCompleto { get; set; } = null!;
        public List<LegajoDetalleDto> Documentos { get; set; } = new List<LegajoDetalleDto>();
    }
}