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
            List<int> rolesUsuarioIds = await _context.UsuariosRoles
                .Where(ur => ur.IdUsuario == idUsuario)
                .Select(ur => ur.IdRol)
                .ToListAsync();

            if (!rolesUsuarioIds.Any())
            {
                return new List<DocumentoFaltanteDto>(); // Si no tiene roles, no se le exigen documentos
            }

            // 2. Obtener qué tipos de documentos se exigen para los roles del usuario
            List<RolesTiposDocumento> documentosRequeridos = await _context.RolesTiposDocumentos
                .Include(rtd => rtd.IdTipoDocNavigation)
                .Where(rtd => rolesUsuarioIds.Contains(rtd.IdRol))
                .ToListAsync();

            // 3. Los documentos que el usuario ya subió
            List<Legajo> legajos = await _context.Legajos
                .Where(l => l.IdUsuario == idUsuario)
                .ToListAsync();

            // 4. De los requeridos, los que todavía no tiene vigentes
            return CalcularFaltantes(documentosRequeridos, legajos, DateTime.Now)
                .Select(grupo => new DocumentoFaltanteDto
                {
                    IdTipoDocumento = grupo.Key,
                    NombreDocumento = grupo.First().IdTipoDocNavigation.NombreDocumento,
                    // Si tiene varios roles, alcanza con que uno lo exija como obligatorio
                    EsObligatorio = grupo.Any(r => r.Obligatorio)
                })
                .ToList();
        }

        /// <summary>
        /// Regla única de "documento faltante", compartida por la pantalla de
        /// faltantes y por el aviso automático por mail.
        ///
        /// Un documento cuenta como entregado si fue subido y no está rechazado.
        /// Si es ANUAL (Apto Médico, Ley 9680, etc.), además tiene que haberse
        /// subido en el año en curso: el del año anterior ya no vale.
        ///
        /// Devuelve las reglas faltantes agrupadas por tipo de documento (un mismo
        /// documento puede exigirse a más de un rol del usuario).
        /// </summary>
        public static List<IGrouping<int, RolesTiposDocumento>> CalcularFaltantes(
            IEnumerable<RolesTiposDocumento> reglas, IEnumerable<Legajo> legajos, DateTime hoy)
        {
            List<RolesTiposDocumento> listaReglas = reglas.ToList();

            // Si para alguno de sus roles el documento es anual, se lo trata como anual
            HashSet<int> tiposAnuales = listaReglas
                .Where(r => r.Anual)
                .Select(r => r.IdTipoDoc)
                .ToHashSet();

            HashSet<int> tiposVigentes = legajos
                .Where(l => l.Estado != "Rechazado")
                .Where(l => !tiposAnuales.Contains(l.IdTipoDoc) || l.FechaCarga.Year == hoy.Year)
                .Select(l => l.IdTipoDoc)
                .ToHashSet();

            return listaReglas
                .Where(r => !tiposVigentes.Contains(r.IdTipoDoc))
                .GroupBy(r => r.IdTipoDoc)
                .ToList();
        }
    }
}
