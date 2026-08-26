using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LegajosController : ControllerBase
    {
        private readonly TuDbContext _context; 
        private readonly IWebHostEnvironment _env;

        public LegajosController(TuDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; 
        }

        // 1. POST: Subir un documento del legajo
        [HttpPost]
        public async Task<IActionResult> SubirDocumento([FromForm] SubirLegajoDto dto) 
        {
            
            if (dto.Archivo == null || dto.Archivo.Length == 0)
                return BadRequest(new { message = "No se ha adjuntado ningún archivo." });

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.IdUsuario);
            var tipoDocExiste = await _context.TiposDocumentos.AnyAsync(t => t.IdTipoDoc == dto.IdTipoDoc);

            if (!usuarioExiste || !tipoDocExiste)
                return NotFound(new { message = "El usuario o el tipo de documento no existen." });

            string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Archivo.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Archivo.CopyToAsync(fileStream);
            }

            string rutaRelativa = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

            var nuevoLegajo = new Legajo
            {
                IdUsuario = dto.IdUsuario,
                IdTipoDoc = dto.IdTipoDoc,
                RutaArchivo = rutaRelativa,
                FechaCarga = DateTime.Now,
                FechaVencimiento = dto.FechaVencimiento,
                Estado = "Pendiente", 
                PresentadoFisico = dto.PresentadoFisico,
                Comentario = null
            };

            _context.Legajos.Add(nuevoLegajo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Documento subido con éxito.", legajo = nuevoLegajo });
        }

        // 2. GET: Obtener todos los documentos de un usuario
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetLegajosPorUsuario(int idUsuario)
        {
            var legajos = await _context.Legajos
                .Where(l => l.IdUsuario == idUsuario)
                .Include(l => l.IdTipoDocNavigation)
                .Include(l => l.IdUsuarioAuditorNavigation)
                .Select(l => new 
                {
                    l.IdLegajo,
                    l.IdUsuario,
                    TipoDocumento = l.IdTipoDocNavigation.NombreDocumento,
                    l.RutaArchivo,
                    l.FechaCarga,
                    l.FechaVencimiento,
                    l.Estado,
                    l.PresentadoFisico,
                    l.Comentario,
                    Auditor = l.IdUsuarioAuditorNavigation != null 
                        ? l.IdUsuarioAuditorNavigation.Nombre + " " + l.IdUsuarioAuditorNavigation.Apellido 
                        : "Sin auditor asignado"
                })
                .ToListAsync();

            if (legajos.Count == 0)
                return NotFound(new { message = "No se encontraron documentos para este usuario." });

            return Ok(legajos);
        }

        // 3. PUT: Auditar documento (Dirección/Secretaría)
        [HttpPut("auditar/{idLegajo}")]
        public async Task<IActionResult> AuditarLegajo(
            int idLegajo,
            [FromQuery] int idUsuarioAuditor, 
            [FromBody] AuditoriaLegajoDto dto
        )
        {
            var legajo = await _context.Legajos.FindAsync(idLegajo);
            if (legajo == null)
                return NotFound(new { message = "No se encontró el registro de legajo." });

            var auditorValido = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuarioAuditor);
            if (!auditorValido)
                return BadRequest(new { message = "El ID del usuario auditor no es válido." });

            legajo.IdUsuarioAuditor = idUsuarioAuditor;
            legajo.Estado = dto.Estado; 
            legajo.Comentario = dto.Comentario;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Auditoría de legajo actualizada correctamente.", legajo });
        }

        // 4. GET: Obtener tipos de documentos filtrados por el ID del rol
        // IMPORTANTE: Este endpoint requiere que la tabla 'roles_tipos_documentos' 
        // tenga datos cargados previamente. (Ej: INSERT INTO roles_tipos_documentos...)
        //DEJO UNOS EJEMPLOS RECUERDEN QUE DEBE HABER ROLES CARGADOS
        // -- 1. Para un Docente (Supongamos IdRol = 2)
        // -- DNI (IdTipoDoc = 1): Es obligatorio (1) pero NO es anual (0)
        // INSERT INTO roles_tipos_documentos (id_rol, id_tipo_doc, obligatorio, anual) VALUES (2, 1, 1, 0);
        //
        // -- Apto Médico (IdTipoDoc = 4): Es obligatorio (1) y SÍ es anual (1)
        // INSERT INTO roles_tipos_documentos (id_rol, id_tipo_doc, obligatorio, anual) VALUES (2, 4, 1, 1);
        //
        // -- 2. Para un Alumno (Supongamos IdRol = 3)
        // -- Analítico Secundario (IdTipoDoc = 10): Es obligatorio (1) pero NO es anual (0)
        // INSERT INTO roles_tipos_documentos (id_rol, id_tipo_doc, obligatorio, anual) VALUES (3, 10, 1, 0);

        [HttpGet("requeridos-por-rol/{idRol}")]
        public async Task<IActionResult> GetDocumentosRequeridos(int idRol)
        {
            
            var consulta = await _context.RolesTiposDocumentos
                .Where(rtd => rtd.IdRol == idRol)
                .Select(rtd => new 
                {
                    NombreRol = rtd.IdRolNavigation.Rol, 
                    
                    IdTipoDoc = rtd.IdTipoDoc,
                    NombreDocumento = rtd.IdTipoDocNavigation.NombreDocumento,
                    Obligatorio = rtd.Obligatorio,
                    Anual = rtd.Anual
                })
                .ToListAsync();

            if (consulta.Count == 0)
                return NotFound(new { message = "No se encontraron documentos configurados para este rol." });


            var respuesta = new 
            {
                Rol = consulta.First().NombreRol, 
                Documentos = consulta.Select(c => new 
                {
                    c.IdTipoDoc,
                    c.NombreDocumento,
                    c.Obligatorio,
                    c.Anual
                })
            };

            return Ok(respuesta);
        }
    }

    
}