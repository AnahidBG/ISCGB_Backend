using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using Microsoft.AspNetCore.Authorization; // Si utiliza JWT
using System.Security.Claims;
using AutoGestionAPI.Seguridad;
using AutoGestionAPI.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfiguracionCalendarioController : ControllerBase
    {
        private readonly TuDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ConfiguracionCalendarioController> _logger;

        public ConfiguracionCalendarioController(
            TuDbContext context,
            IEmailService emailService,
            ILogger<ConfiguracionCalendarioController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // Estados de la convocatoria de un docente a una mesa (tabla mesa_examen)
        private const string MesaPendiente = "Pendiente";
        private const string MesaAceptada = "Aceptada";

        /// <summary>
        /// Exámenes que OCUPAN un lugar del límite diario: los que cargan los docentes
        /// y las mesas con alguna convocatoria pendiente o aceptada. Una mesa en la que
        /// todos los convocados rechazaron no ocupa lugar.
        /// </summary>
        private IQueryable<Examene> ExamenesQueOcupanElDia()
        {
            return _context.Examenes.Where(e =>
                !_context.MesasExamenes.Any(m => m.IdExamen == e.IdExamen) ||
                _context.MesasExamenes.Any(m => m.IdExamen == e.IdExamen &&
                    (m.EstadoConfirmacion == MesaPendiente || m.EstadoConfirmacion == MesaAceptada)));
        }

        /// <summary>
        /// Exámenes que se MUESTRAN en el calendario: los que cargan los docentes y
        /// las mesas recién cuando un docente convocado las aceptó.
        /// </summary>
        private IQueryable<Examene> ExamenesVisiblesEnCalendario()
        {
            return _context.Examenes.Where(e =>
                !_context.MesasExamenes.Any(m => m.IdExamen == e.IdExamen) ||
                _context.MesasExamenes.Any(m => m.IdExamen == e.IdExamen && m.EstadoConfirmacion == MesaAceptada));
        }

        /// <summary>
        /// Le avisa por mail al docente que lo convocaron a una mesa. Si el mail falla,
        /// la convocatoria igual queda registrada (el docente la ve en el sistema).
        /// </summary>
        private async Task AvisarConvocatoriaAsync(int idDocente, int idExamen)
        {
            try
            {
                var datos = await _context.Examenes
                    .Where(e => e.IdExamen == idExamen)
                    .Select(e => new { Materia = e.IdMateriaNavigation.Nombre, e.Fecha })
                    .FirstAsync();
                Usuario usuario = await _context.Docentes
                    .Where(d => d.IdDocente == idDocente)
                    .Select(d => d.IdUsuarioNavigation)
                    .FirstAsync();

                await _emailService.EnviarAvisoMesaExamenAsync(usuario.Email, usuario.Nombre ?? "Docente", datos.Materia, datos.Fecha);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el aviso de la mesa {IdExamen} al docente {IdDocente}.", idExamen, idDocente);
            }
        }

        /// <summary>
        /// Tipos de examen que un docente carga y puede modificar desde el calendario.
        /// </summary>
        private static readonly string[] TiposQueGestionaElDocente = { "Parcial", "Recuperatorio", "Final" };

        /// <summary>
        /// Dirección/Secretaría pueden modificar o eliminar cualquier examen.
        /// Un docente, solo los que cargó él mismo y que sean Parcial, Recuperatorio
        /// o Final (no las mesas que registra Dirección/Secretaría).
        /// </summary>
        private async Task<bool> PuedeGestionarExamenAsync(Examene examen)
        {
            if (User.EsAdministrativo()) return true;

            int? idDocente = await ObtenerIdDocenteActualAsync();
            if (idDocente == null || examen.IdDocenteCreador != idDocente) return false;

            string? tipo = await _context.TipoExamen
                .Where(t => t.IdTipoExamen == examen.IdTipoExamen)
                .Select(t => t.TipoExamen)
                .FirstOrDefaultAsync();

            return tipo != null && TiposQueGestionaElDocente.Contains(tipo);
        }

        /// <summary>El IdDocente del usuario logueado, o null si no es docente.</summary>
        private async Task<int?> ObtenerIdDocenteActualAsync()
        {
            if (!User.IsInRole("Docente")) return null;

            int? idUsuario = User.ObtenerIdUsuario();
            if (idUsuario == null) return null;

            return await _context.Docentes
                .Where(d => d.IdUsuario == idUsuario.Value)
                .Select(d => (int?)d.IdDocente)
                .FirstOrDefaultAsync();
        }

        // Si usa JWT, puede descomentar la siguiente línea para restringir el acceso:
        [Authorize(Roles = "Director, Secretario")]
        [HttpPut("limite-parciales")]
        public async Task<IActionResult> ActualizarLimiteParciales([FromBody] ActualizarLimiteParcialesDto dto)
        {
            // 1. Validación básica: un límite no puede ser negativo
            if (dto.NuevoLimite <= 0)
            {
                return BadRequest(new { message = "El límite de parciales Diarios debe ser mayor a cero." });
            }

            // 2. Buscamos el registro de configuración (asumimos que siempre hay uno principal)
            ConfiguracionSistema? config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            if (config == null)
            {
                // Si por alguna razón la tabla estuviera vacía, creamos el primer registro
                config = new ConfiguracionSistema
                {
                    LimiteParcialesDiario = dto.NuevoLimite,
                    // Si tiene otras propiedades obligatorias (como FrecuenciaNotificacionDias), agréguelas aquí.
                };
                _context.ConfiguracionesSistema.Add(config);
            }
            else
            {
                // 3. Si existe, simplemente actualizamos el valor
                config.LimiteParcialesDiario = dto.NuevoLimite;
            }

            // 4. Guardamos los cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "El límite de parciales por día se ha actualizado con éxito.",
                nuevoLimite = config.LimiteParcialesDiario
            });
        }
        /// <summary>
        /// El límite diario se aplica por GRUPO DE ALUMNOS: año (curso de la materia)
        /// + comisión. Ej.: con límite 2, un día puede tener 2 exámenes de Primer Año
        /// comisión A, 2 de Primer Año comisión B, 2 de Segundo Año comisión A, etc.
        /// </summary>
        private async Task<bool> SuperaLimiteDiarioAsync(DateTime fechaDeseada, int idMateria, int idComision, int? idExamenIgnorar = null)
        {
            // 1. Obtener el límite configurado (por defecto 2)
            ConfiguracionSistema? config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();
            int limite = config?.LimiteParcialesDiario ?? 2;

            // El año de la materia (puede no estar cargado: esas materias forman su propio grupo)
            string? curso = await _context.Materias
                .Where(m => m.IdMateria == idMateria)
                .Select(m => m.Curso)
                .FirstOrDefaultAsync();

            // 2. Contar cuántos exámenes de ese año y comisión ocupan esa fecha (ignorando
            //    la hora). Incluye mesas pendientes y aceptadas; no las rechazadas.
            IQueryable<Examene> query = ExamenesQueOcupanElDia()
                .Where(e => e.Fecha.Date == fechaDeseada.Date
                    && e.IdComision == idComision
                    && e.IdMateriaNavigation.Curso == curso);

            // Si estamos editando, ignoramos el examen actual
            if (idExamenIgnorar.HasValue)
            {
                query = query.Where(e => e.IdExamen != idExamenIgnorar.Value);
            }

            int examenesEnElDia = await query.CountAsync();

            // 3. Retornar si agregar uno más superaría el límite diario
            return (examenesEnElDia + 1) > limite;
        }

        [HttpPost("registrar")]
        [Authorize(Roles = "Docente")] // Aseguramos que solo entre un docente
        public async Task<IActionResult> RegistrarExamen([FromBody] CargaExamenDto dto)
        {
            // 1. Identificamos al docente logueado
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            Docente? docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null) return NotFound(new { message = "Docente no encontrado." });

            // 2. MAGIA DE SEGURIDAD: Verificamos en la tabla docente_materia si realmente da esa clase
            // (Ajuste "DocenteMaterias" al nombre exacto que le haya puesto Entity Framework a su tabla intermedia)
            bool dictaLaMateria = await _context.DocenteMateria
                .AnyAsync(dm => dm.IdDocente == docente.IdDocente && dm.IdMateria == dto.IdMateria);

            if (!dictaLaMateria)
            {
                return BadRequest(new { message = "No tiene autorización para registrar exámenes de una materia que no dicta." });
            }

            string? tipoExamen = await _context.TipoExamen
                .Where(t => t.IdTipoExamen == dto.IdTipoExamen)
                .Select(t => t.TipoExamen)
                .FirstOrDefaultAsync();
            if (tipoExamen == null || !TiposQueGestionaElDocente.Contains(tipoExamen))
            {
                return BadRequest(new { message = "Tipo de examen no válido. Debe ser Parcial, Recuperatorio o Final." });
            }

            // 3. Verificamos el límite diario que ya teníamos programado
            bool superaLimite = await SuperaLimiteDiarioAsync(dto.Fecha, dto.IdMateria, dto.IdComision);
            if (superaLimite)
            {
                return BadRequest(new { message = "No se puede registrar: ese año y comisión ya tienen el máximo de exámenes permitido para ese día." });
            }

            // 4. Guardamos el examen (tal como lo tenía)
            Examene nuevoExamen = new Examene
            {
                Fecha = dto.Fecha,
                IdComision = dto.IdComision,
                IdMateria = dto.IdMateria,
                IdTipoExamen = dto.IdTipoExamen,
                IdDocenteCreador = docente.IdDocente
            };

            _context.Examenes.Add(nuevoExamen);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Examen registrado correctamente.",
                idExamen = nuevoExamen.IdExamen
            });
        }

        /// <summary>
        /// Ocupación de cada día del mes. El límite es por año + comisión:
        /// - Con <c>idMateria</c> e <c>idComision</c> (la materia y comisión que se
        ///   están por cargar), cada día informa la cantidad y si está lleno PARA ESE
        ///   año y comisión. Es lo que debería usar la pantalla de carga.
        /// - Sin ellos, un día figura lleno si algún año + comisión llegó al límite.
        /// En ambos casos cada día trae el detalle en <c>porGrupo</c>.
        /// </summary>
        [HttpGet("estado-mes")]
        public async Task<IActionResult> ObtenerEstadoMes(int anio, int mes, [FromQuery] int? idMateria = null, [FromQuery] int? idComision = null)
        {
            ConfiguracionSistema? config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();
            int limite = config?.LimiteParcialesDiario ?? 2;

            DateTime fechaInicioMes = new DateTime(anio, mes, 1);
            DateTime fechaFinMes = fechaInicioMes.AddMonths(1).AddSeconds(-1);

            // Días ocupados según el límite: cuentan también las mesas pendientes,
            // para que nadie cargue un examen en un día que ya está reservado.
            var examenesDelMes = await ExamenesQueOcupanElDia()
                .Where(e => e.Fecha >= fechaInicioMes && e.Fecha <= fechaFinMes)
                .Select(e => new
                {
                    e.Fecha,
                    Curso = e.IdMateriaNavigation.Curso,
                    e.IdComision,
                    Comision = e.IdComisionNavigation.Comision1
                })
                .ToListAsync();

            // Si piden un año + comisión concreto, solo cuentan los exámenes de ese grupo
            bool filtrarGrupo = idMateria.HasValue && idComision.HasValue;
            string? cursoFiltro = null;
            if (filtrarGrupo)
            {
                cursoFiltro = await _context.Materias
                    .Where(m => m.IdMateria == idMateria!.Value)
                    .Select(m => m.Curso)
                    .FirstOrDefaultAsync();
                examenesDelMes = examenesDelMes
                    .Where(e => e.IdComision == idComision!.Value && e.Curso == cursoFiltro)
                    .ToList();
            }

            var estadoPorDia = examenesDelMes
                .GroupBy(e => e.Fecha.Date)
                .Select(dia =>
                {
                    var porGrupo = dia
                        .GroupBy(e => new { e.Curso, e.IdComision, e.Comision })
                        .Select(g => new
                        {
                            Curso = g.Key.Curso,
                            IdComision = g.Key.IdComision,
                            Comision = g.Key.Comision,
                            CantidadExamenes = g.Count(),
                            LimiteAlcanzado = g.Count() >= limite
                        })
                        .ToList();

                    return new
                    {
                        Fecha = dia.Key,
                        CantidadExamenes = dia.Count(),
                        LimiteAlcanzado = porGrupo.Any(g => g.LimiteAlcanzado),
                        PorGrupo = porGrupo
                    };
                });

            return Ok(new
            {
                limiteDiario = limite,
                diasOcupados = estadoPorDia
            });
        }
        // --- 1. ENDPOINT PARA MODIFICAR ---
        [HttpPut("modificar/{idExamen}")]
        public async Task<IActionResult> ModificarExamen(int idExamen, [FromBody] ModificarFechaExamenDto dto)
        {
            Examene? examen = await _context.Examenes.FindAsync(idExamen);
            if (examen == null)
            {
                return NotFound(new { message = "No se encontró el examen especificado." });
            }

            if (!await PuedeGestionarExamenAsync(examen))
                return Forbid();

            // Validamos la regla de negocio, pasándole el ID del examen actual 
            // para que NO se cuente a sí mismo en el cálculo de la semana.
            bool superaLimite = await SuperaLimiteDiarioAsync(dto.NuevaFecha, examen.IdMateria, examen.IdComision, idExamen);

            if (superaLimite)
            {
                return BadRequest(new { message = "No se puede reprogramar: ese año y comisión ya tienen el máximo de exámenes permitido para ese día." });
            }

            // Si pasa la validación, actualizamos la fecha
            examen.Fecha = dto.NuevaFecha;
            await _context.SaveChangesAsync();

            return Ok(new { message = "La fecha del examen se ha modificado correctamente." });
        }


        // --- 2. ENDPOINT PARA ELIMINAR ---
        [HttpDelete("eliminar/{idExamen}")]
        public async Task<IActionResult> EliminarExamen(int idExamen)
        {
            Examene? examen = await _context.Examenes.FindAsync(idExamen);
            if (examen == null)
            {
                return NotFound(new { message = "No se encontró el examen especificado." });
            }

            if (!await PuedeGestionarExamenAsync(examen))
                return Forbid();

            // La base no borra en cascada los docentes asignados a la mesa: si no se
            // quitan antes, el DELETE falla por la clave foránea de mesa_examen.
            List<MesaExamen> asignaciones = await _context.MesasExamenes
                .Where(m => m.IdExamen == idExamen)
                .ToListAsync();
            _context.MesasExamenes.RemoveRange(asignaciones);

            _context.Examenes.Remove(examen);
            await _context.SaveChangesAsync();   // ambos borrados en una sola transacción

            return Ok(new { message = "El examen ha sido eliminado del calendario correctamente." });
        }
        [HttpGet("tipos-examen")]
        public async Task<IActionResult> GetTiposExamen()
        {
            var tipos = await _context.TipoExamen
                .Select(t => new
                {
                    idTipoExamen = t.IdTipoExamen,
                    nombre = t.TipoExamen
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("mis-materias-docente")]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> ObtenerMisMateriasDocente()
        {
            // 1. Extraemos el ID del usuario desde el Token JWT
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out int idUsuario))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // 2. Verificamos que este usuario realmente sea un Docente
            Docente? docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);

            if (docente == null)
            {
                return NotFound(new { message = "El usuario no tiene un perfil de docente." });
            }

            // 3. Cruzamos la tabla intermedia con Materias y Comisiones
            var materiasAsignadas = await (
                from dm in _context.DocenteMateria
                join m in _context.Materias on dm.IdMateria equals m.IdMateria
                join c in _context.Comisions on dm.IdComision equals c.IdComision
                where dm.IdDocente == docente.IdDocente
                select new
                {
                    IdMateria = m.IdMateria,
                    NombreMateria = m.Nombre,
                    Carrera = m.Carrera,
                    IdComision = c.IdComision,
                    NombreComision = c.Comision1
                }
            ).Distinct().ToListAsync();

            return Ok(materiasAsignadas);
        }

        [HttpGet("del-mes")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ExamenMesDto>>> ObtenerExamenesDelMes()
        {
            int mesActual = DateTime.Now.Month;
            int anioActual = DateTime.Now.Year;

            bool esAdministrativo = User.EsAdministrativo();
            int? idDocente = await ObtenerIdDocenteActualAsync();

            // Las mesas aparecen recién cuando el docente convocado las acepta
            List<ExamenMesDto> examenesMes = await ExamenesVisiblesEnCalendario()
                .Include(e => e.IdMateriaNavigation)
                .Include(e => e.IdComisionNavigation)    // NUEVO: Traemos los datos de la Comisión
                .Include(e => e.IdTipoExamenNavigation)  // NUEVO: Traemos los datos del Tipo de Examen
                .Where(e => e.Fecha.Month == mesActual && e.Fecha.Year == anioActual)
                .Select(e => new ExamenMesDto
                {
                    IdExamen = e.IdExamen,
                    Materia = e.IdMateriaNavigation.Nombre,
                    Fecha = e.Fecha,

                    // Ajuste ".Nombre" por el campo exacto que tenga en sus clases (ej. .Descripcion, .NumeroComision, etc.)
                    Comision = e.IdComisionNavigation.Comision1,
                    TipoExamen = e.IdTipoExamenNavigation.TipoExamen,
                    PuedeModificar = esAdministrativo
                        || (idDocente != null
                            && e.IdDocenteCreador == idDocente
                            && TiposQueGestionaElDocente.Contains(e.IdTipoExamenNavigation.TipoExamen))
                })
                .OrderBy(e => e.Fecha)
                .ToListAsync();

            if (!examenesMes.Any())
            {
                return Ok(new List<ExamenMesDto>());
            }

            return Ok(examenesMes);
        }

        // --- 3. ENDPOINT PARA ASIGNAR DOCENTE A MESA DE EXAMEN YA CREADA ---
        [HttpPost("asignar-docente")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> AsignarDocenteAMesa([FromBody] AsignarMesaDto dto)
        {
            Examene? examen = await _context.Examenes.FindAsync(dto.IdExamen);
            if (examen == null)
                return NotFound(new { message = "No se encontró la mesa de examen." });

            if (!await _context.Docentes.AnyAsync(d => d.IdDocente == dto.IdDocente))
                return BadRequest(new { message = "El docente seleccionado no existe." });

            // 1. Verificamos que no exista ya una asignación para este examen y este docente
            bool asignacionExiste = await _context.MesasExamenes
                .AnyAsync(m => m.IdExamen == dto.IdExamen && m.IdDocente == dto.IdDocente);

            if (asignacionExiste)
            {
                return BadRequest(new { message = "El docente ya se encuentra asignado a esta mesa de examen." });
            }

            // Si la mesa había quedado libre (todos rechazaron), al reasignarla vuelve
            // a ocupar el día: tiene que haber lugar dentro del límite diario.
            bool yaOcupaElDia = await ExamenesQueOcupanElDia().AnyAsync(e => e.IdExamen == dto.IdExamen);
            if (!yaOcupaElDia && await SuperaLimiteDiarioAsync(examen.Fecha, examen.IdMateria, examen.IdComision, examen.IdExamen))
            {
                return BadRequest(new { message = "No se puede reasignar la mesa: ese año y comisión ya tienen el máximo de exámenes permitido para ese día." });
            }

            // 2. Creamos la asignación
            MesaExamen nuevaAsignacion = new MesaExamen // Ajuste el nombre exacto de su clase si es singular o plural
            {
                IdExamen = dto.IdExamen,
                IdDocente = dto.IdDocente,
                EstadoConfirmacion = "Pendiente" // Estado inicial automático
            };

            _context.MesasExamenes.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            await AvisarConvocatoriaAsync(dto.IdDocente, dto.IdExamen);

            return Ok(new { message = "Docente asignado correctamente. Se ha notificado al profesor." });
        }

        //Endpoint para crear mesa de examen y asignar al presidente de mesa.

        [HttpPost("registrar-mesa")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> RegistrarMesa([FromBody] RegistrarMesaDto dto)
        {

            bool docenteExiste = await _context.Docentes.AnyAsync(d => d.IdDocente == dto.IdDocente);
            if (!docenteExiste)
            {
                return BadRequest(new { message = "El docente seleccionado no existe." });
            }

            // La mesa también cuenta para el límite de exámenes por día
            if (await SuperaLimiteDiarioAsync(dto.Fecha, dto.IdMateria, dto.IdComision))
            {
                return BadRequest(new { message = "No se puede registrar la mesa: ese año y comisión ya tienen el máximo de exámenes permitido para ese día." });
            }

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso A: Insertamos los datos de la fecha y materia en Examenes
                Examene nuevaMesa = new Examene
                {
                    Fecha = dto.Fecha,
                    IdMateria = dto.IdMateria,
                    IdComision = dto.IdComision,
                    IdTipoExamen = dto.IdTipoExamen
                };

                _context.Examenes.Add(nuevaMesa);
                await _context.SaveChangesAsync();


                MesaExamen asignacionDocente = new MesaExamen
                {
                    IdExamen = nuevaMesa.IdExamen,
                    IdDocente = dto.IdDocente,
                    EstadoConfirmacion = "Pendiente"
                };

                _context.MesasExamenes.Add(asignacionDocente);
                await _context.SaveChangesAsync();

                // Confirmamos definitivamente ambos cambios en SQL Server
                await transaction.CommitAsync();

                await AvisarConvocatoriaAsync(dto.IdDocente, nuevaMesa.IdExamen);

                return Ok(new
                {
                    message = "Mesa de examen registrada exitosamente. Se ha enviado la notificación pendiente al docente.",
                    idExamen = nuevaMesa.IdExamen
                });
            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Ocurrió un error al registrar la mesa de examen." });
            }
        }

    }
}

