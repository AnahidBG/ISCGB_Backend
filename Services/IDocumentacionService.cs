using AutoGestionAPI.DTOs.Legajos;

namespace AutoGestionAPI.Services
{
    public interface IDocumentacionService
    {
        Task<List<DocumentoFaltanteDto>> ObtenerDocumentacionFaltanteAsync(int idUsuario);
    }
}