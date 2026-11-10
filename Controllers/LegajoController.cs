using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using AutoGestionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using AutoGestionAPI.Seguridad;
using System.Globalization;
using AutoGestionAPI.DTOs.Legajos;
using System.Text;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LegajosController : ControllerBase
    {
        private readonly TuDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDocumentacionService _documentacionService;
        private readonly IEmailService _emailService;
        private readonly ILogger<LegajosController> _logger;

        public LegajosController(
            TuDbContext context,
            IWebHostEnvironment env,
            IDocumentacionService documentacionService,
            IEmailService emailService,
            ILogger<LegajosController> logger)
        {
            _context = context;
            _env = env;
            _documentacionService = documentacionService;
            _emailService = emailService;
            _logger = logger;
        }

        // 1. POST: Subir un documento del legajo
        [HttpPost]
        public async Task<IActionResult> SubirDocumento([FromForm] SubirLegajoDto dto)
        {
            // Solo se puede subir documentación propia (salvo Dirección/Secretaría)
            if (!User.PuedeAccederA(dto.IdUsuario))
                return Forbid();

            // Valida extensión (PDF/JPG/PNG), tamaño máximo y contenido real del archivo
            string? errorArchivo = ValidadorArchivos.Validar(dto.Archivo, ValidadorArchivos.PdfOImagen);
            if (errorArchivo != null)
                return BadRequest(new { message = errorArchivo });


            Usuario? usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            TiposDocumento? tipoDocumento = await _context.TiposDocumentos.FindAsync(dto.IdTipoDoc);

            if (usuario == null || tipoDocumento == null)
                return NotFound(new { message = "El usuario o el tipo de documento no existen." });


            string nombreLimpio = LimpiarCadena($"{usuario.Nombre}{usuario.Apellido}");
            string tipoDocLimpio = LimpiarCadena(tipoDocumento.NombreDocumento);


            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string extension = ValidadorArchivos.ObtenerExtension(dto.Archivo!);
            // Código aleatorio al final para que la URL del archivo no se pueda adivinar
            string codigo = Guid.NewGuid().ToString("N")[..12];

            string nombreArchivoFinal = $"ISCGB_{nombreLimpio}_{tipoDocLimpio}_{timestamp}_{codigo}{extension}";


            string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "legajos");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, nombreArchivoFinal);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Archivo.CopyToAsync(fileStream);
            }

            string rutaRelativa = $"/uploads/legajos/{nombreArchivoFinal}";

            Legajo nuevoLegajo = new Legajo
            {
                IdUsuario = dto.IdUsuario,
                IdTipoDoc = dto.IdTipoDoc,
                RutaArchivo = rutaRelativa,
                FechaCarga = DateTime.Now,
                FechaVencimiento = dto.FechaVencimiento,
                Estado = "Pendiente",
                PresentadoFisico = null,
                Comentario = null
            };

            _context.Legajos.Add(nuevoLegajo);
            await _context.SaveChangesAsync();

            return Ok(new RespuestaCargaDto
            {
                Message = "Documento subido con éxito.",
                IdLegajo = nuevoLegajo.IdLegajo,
                Ruta = nuevoLegajo.RutaArchivo
            });
        }

        private static bool EsRechazado(string? estado)
        {
            return string.Equals(estado?.Trim(), "Rechazado", StringComparison.OrdinalIgnoreCase);
        }

        //Método para borrar acentos, Ñ, espacios en blanco
        private string LimpiarCadena(string texto)
        {
            return ValidadorArchivos.LimpiarParaNombreArchivo(texto);
        }

        // 2. GET: Obtener todos los documentos de un usuario
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetLegajosPorUsuario(int idUsuario)
        {
            if (!User.PuedeAccederA(idUsuario))
                return Forbid();

            Usuario? usuario = await _context.Usuarios
                .Include(u => u.LegajoIdUsuarioNavigations)
                    .ThenInclude(l => l.IdTipoDocNavigation)
                .Include(u => u.LegajoIdUsuarioNavigations)
                    .ThenInclude(l => l.IdUsuarioAuditorNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return NotFound(new { message = "No se encontró el usuario." });

            UsuarioDocumentosDto respuesta = new UsuarioDocumentosDto
            {
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}".Trim(),

                Documentos = usuario.LegajoIdUsuarioNavigations.Select(l => new LegajoDetalleDto
                {
                    IdLegajo = l.IdLegajo,
                    IdUsuario = l.IdUsuario,
                    TipoDocumento = l.IdTipoDocNavigation?.NombreDocumento ?? "Documento sin tipo",
                    RutaArchivo = l.RutaArchivo ?? "",
                    FechaCarga = l.FechaCarga,
                    FechaVencimiento = l.FechaVencimiento,
                    Estado = l.Estado ?? "Pendiente",
                    PresentadoFisico = l.PresentadoFisico,
                    Comentario = l.Comentario,
                    Auditor = l.IdUsuarioAuditorNavigation != null
                        ? $"{l.IdUsuarioAuditorNavigation.Nombre} {l.IdUsuarioAuditorNavigation.Apellido}".Trim()
                        : "Sin auditor asignado"
                }).ToList()
            };

            return Ok(respuesta);
        }

        // Auditar documento (Dirección/Secretaría)
        [HttpPut("auditar/{idLegajo}")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> AuditarLegajo(
     int idLegajo,
     [FromQuery] int idUsuarioAuditor,
     [FromBody] AuditoriaLegajoDto dto
 )
        {
            Legajo? legajo = await _context.Legajos.FindAsync(idLegajo);
            if (legajo == null)
                return NotFound(new { message = "No se encontró el registro de legajo." });

            // El auditor es quien está logueado. El parámetro idUsuarioAuditor se
            // mantiene para no romper al frontend, pero ya no se usa.
            int? idAuditor = User.ObtenerIdUsuario();
            if (idAuditor == null)
                return Unauthorized();

            bool recienRechazado = EsRechazado(dto.Estado) && !EsRechazado(legajo.Estado);

            legajo.IdUsuarioAuditor = idAuditor.Value;
            legajo.Estado = dto.Estado;
            legajo.Comentario = dto.Comentario;
            legajo.PresentadoFisico = dto.PresentadoFisico;

            await _context.SaveChangesAsync();

            // Si se acaba de rechazar, se le avisa por mail con el motivo (el comentario).
            // Si el mail falla, la auditoría ya quedó guardada igual.
            if (recienRechazado)
            {
                try
                {
                    var datos = await _context.Legajos
                        .Where(l => l.IdLegajo == idLegajo)
                        .Select(l => new
                        {
                            l.IdUsuarioNavigation.Email,
                            l.IdUsuarioNavigation.Nombre,
                            Documento = l.IdTipoDocNavigation.NombreDocumento
                        })
                        .FirstAsync();

                    await _emailService.EnviarAvisoRechazoAsync(
                        datos.Email,
                        datos.Nombre ?? "Usuario",
                        $"tu documento \"{datos.Documento}\"",
                        dto.Comentario,
                        "Ingresá al sistema y volvé a subirlo desde tu legajo.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo enviar el aviso de rechazo del legajo {IdLegajo}.", idLegajo);
                }
            }

            return Ok(new RespuestaAuditoriaDto
            {
                Message = "Auditoría de legajo actualizada correctamente.",
                IdLegajo = legajo.IdLegajo,
                Estado = legajo.Estado,
                PresentadoFisico = legajo.PresentadoFisico
            });
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
        // ENDPOINT QUE TRAE TODOS LOS DOCUMENTOS EN ESTADO PENDIENTE
        [HttpGet("pendientes")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> ObtenerLegajosPendientes()
        {
            List<LegajoPendienteDto> pendientes = await _context.Legajos
            .Where(l => l.Estado == "Pendiente")
            .Select(l => new LegajoPendienteDto
            {
                IdLegajo = l.IdLegajo,
                NombreUsuario = l.IdUsuarioNavigation.Nombre + " " + l.IdUsuarioNavigation.Apellido,

                TipoDocumento = l.IdTipoDocNavigation.NombreDocumento,

                RutaArchivo = l.RutaArchivo,
                FechaCarga = l.FechaCarga,
                PresentadoFisico = l.PresentadoFisico
            }).ToListAsync();

            if (!pendientes.Any())
            {
                return Ok(new List<LegajoPendienteDto>());
            }

            return Ok(pendientes);
        }

        //ENDPOINT QUE TRAE TODOS LOS DOCUMENTOS SUBIDOS POR USUARIO
        [HttpGet("resumen-estado")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> ObtenerResumenEstado()
        {
            List<Usuario> usuarios = await _context.Usuarios
                .Include(u => u.LegajoIdUsuarioNavigations)
                .ToListAsync();

            List<ResumenLegajoDto> resumen = usuarios.Select(usuario => new ResumenLegajoDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Dni = usuario.Dni ?? "Sin DNI",


                Documentos = usuario.LegajoIdUsuarioNavigations.Select(l => new DocumentoSubidoDto
                {
                    IdLegajo = l.IdLegajo,
                    IdTipoDoc = l.IdTipoDoc,
                    Estado = l.Estado,
                    RutaArchivo = l.RutaArchivo
                }).ToList()
            }).ToList();

            return Ok(resumen);
        }

        //Notificación de documentos faltantes para un usuario específico
        [HttpGet("{idUsuario}/faltantes")]
        public async Task<IActionResult> ObtenerFaltantes(int idUsuario)
        {
            if (!User.PuedeAccederA(idUsuario))
                return Forbid();

            List<DocumentoFaltanteDto> faltantes = await _documentacionService.ObtenerDocumentacionFaltanteAsync(idUsuario);

            if (!faltantes.Any())
            {
                return Ok(new { message = "El legajo está completo. No falta documentación." });
            }

            return Ok(faltantes);
        }

        [HttpGet("aprobados")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> ObtenerLegajosAprobados()
        {
            List<LegajoAprobadoDto> aprobados = await _context.Legajos
                .Where(l => l.Estado == "Aprobado")
                .Select(l => new LegajoAprobadoDto
                {
                    IdLegajo = l.IdLegajo,
                    NombreUsuario = l.IdUsuarioNavigation.Nombre + " " + l.IdUsuarioNavigation.Apellido,
                    TipoDocumento = l.IdTipoDocNavigation.NombreDocumento,
                    RutaArchivo = l.RutaArchivo,
                    FechaCarga = l.FechaCarga,

                    PresentadoFisico = l.PresentadoFisico,

                    FechaVencimiento = l.FechaVencimiento,
                    Comentario = l.Comentario,


                    Auditor = l.IdUsuarioAuditorNavigation != null
                                ? l.IdUsuarioAuditorNavigation.Nombre + " " + l.IdUsuarioAuditorNavigation.Apellido
                                : "Sin auditor"
                })
                .ToListAsync();

            return Ok(aprobados);
        }

    }
}