using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs.Legajos;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Services
{
    public class DocumentacionService : IDocumentacionService
    {
        private readonly TuDbContext _context;

        public DocumentacionService(TuDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentoFaltanteDto>> ObtenerDocumentacionFaltanteAsync(int idUsuario)
        {
            // 1. Obtener los IDs de los roles asignados a este usuario
            var rolesUsuarioIds = await _context.UsuariosRoles // Ajustá el nombre si tu DbSet se llama distinto (ej: UsuariosRoles)
                .Where(ur => ur.IdUsuario == idUsuario)
                .Select(ur => ur.IdRol)
                .ToListAsync();

            if (!rolesUsuarioIds.Any())
            {
                return new List<DocumentoFaltanteDto>(); // Si no tiene roles, no se le exigen documentos
            }

            // 2. Obtener qué tipos de documentos se exigen para los roles del usuario
            var documentosRequeridos = await _context.RolesTiposDocumentos
                // CORRECTO: Incluimos solo el objeto de navegación (la tabla relacionada entera)
                .Include(rtd => rtd.IdTipoDocNavigation)
                .Where(rtd => rolesUsuarioIds.Contains(rtd.IdRol))
                .ToListAsync();

            // 3. Obtener los IDs de los documentos que el usuario YA subió y NO están rechazados
            var documentosEntregadosIds = await _context.Legajos
                .Where(l => l.IdUsuario == idUsuario && l.Estado != "Rechazado")
                .Select(l => l.IdTipoDoc)
                .ToListAsync();

            // 4. Filtrar: De los requeridos, sacar los que ya entregó
            var documentosFaltantes = documentosRequeridos
                .Where(req => !documentosEntregadosIds.Contains(req.IdTipoDoc)) // Cruzamos por el ID
                .Select(req => new DocumentoFaltanteDto
                {
                    IdTipoDocumento = req.IdTipoDoc,
                    // ACÁ SÍ accedemos a la propiedad de texto, porque el objeto ya vino incluido en el paso 2
                    NombreDocumento = req.IdTipoDocNavigation.NombreDocumento,
                    EsObligatorio = req.Obligatorio
                })
                .ToList();

            return documentosFaltantes;
        }
    }
}