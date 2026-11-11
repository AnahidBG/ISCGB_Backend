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
        /// Exámenes que se MUESTRAN en el calendario: los que cargan los docentes, y
        /// las mesas recién cuando el tribunal completo aceptó (presidente, vocal 1 y
        /// vocal 2).
        /// </summary>
        private IQueryable<Examene> ExamenesVisiblesEnCalendario()
        {
            int lugaresDelTribunal = RolesMesa.Todos.Length;
            return _context.Examenes.Where(e =>
                !_context.MesasExamenes.Any(m => m.IdExamen == e.IdExamen) ||
                _context.MesasExamenes
                    .Where(m => m.IdExamen == e.IdExamen && m.EstadoConfirmacion == MesaAceptada)
                    .Select(m => m.Rol)
                    .Distinct()
                    .Count() == lugaresDelTribunal);
        }

        /// <summary>
        /// Le avisa por mail al docente que lo convocaron a una mesa. Si el mail falla,
        /// la convocatoria igual queda registrada (el docente la ve en el sistema).
        /// </summary>
        private async Task AvisarConvocatoriaAsync(int idDocente, int idExamen, string rol)
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

                await _emailService.EnviarAvisoMesaExamenAsync(usuario.Email, usuario.Nombre ?? "Docente", datos.Materia, datos.Fecha, rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el aviso de la mesa {IdExamen} al docente {IdDocente}.", idExamen, idDocente);
            }
        }

        /// <summary>
        /// Tipos de examen que un docente carga y puede modificar desde el calendario.
        /// </summary>
        private static readonly string[] TiposQueGestionaElDocente = TiposExamen.DelDocente;

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
        ///
        /// Una mesa de examen no tiene comisión: es del año entero, así que ocupa un
        /// lugar en CADA comisión de ese año. Por eso:
        /// - Un examen de una comisión entra si esa comisión, contando las mesas del
        ///   año, todavía tiene lugar.
        /// - Una mesa (<paramref name="idComision"/> null) entra si TODAS las
        ///   comisiones del año tienen lugar.
        /// </summary>
        private async Task<bool> SuperaLimiteDiarioAsync(DateTime fechaDeseada, int idMateria, int? idComision, int? idExamenIgnorar = null)
        {
            // 1. Obtener el límite configurado (por defecto 2)
            ConfiguracionSistema? config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();
            int limite = config?.LimiteParcialesDiario ?? 2;

            // El año de la materia (puede no estar cargado: esas materias forman su propio grupo)
            string? curso = await _context.Materias
                .Where(m => m.IdMateria == idMateria)
                .Select(m => m.Curso)
                .FirstOrDefaultAsync();

            // 2. Los exámenes de ese año que ocupan esa fecha (ignorando la hora).
            //    Incluye mesas pendientes y aceptadas; no las rechazadas.
            IQueryable<Examene> query = ExamenesQueOcupanElDia()
                .Where(e => e.Fecha.Date == fechaDeseada.Date
                    && e.IdMateriaNavigation.Curso == curso);

            // Si estamos editando, ignoramos el examen actual
            if (idExamenIgnorar.HasValue)
            {
                query = query.Where(e => e.IdExamen != idExamenIgnorar.Value);
            }

            List<int?> comisionesDelDia = await query.Select(e => e.IdComision).ToListAsync();

            // Las mesas del año (sin comisión) cuentan para todas las comisiones
            int mesasDelAnio = comisionesDelDia.Count(c => c == null);

            // La comisión más cargada que hay que respetar: la pedida, o (para una
            // mesa) la que más exámenes tenga ese día
            int examenesDeLaComision = idComision.HasValue
                ? comisionesDelDia.Count(c => c == idComision.Value)
                : comisionesDelDia.Where(c => c != null).GroupBy(c => c).Select(g => g.Count()).DefaultIfEmpty(0).Max();

            // 3. Retornar si agregar uno más superaría el límite diario
            return (mesasDelAnio + examenesDeLaComision + 1) > limite;
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
        /// - Con solo <c>idMateria</c> (por ejemplo, al armar una mesa de examen, que es
        ///   del año entero), cuentan los exámenes de ese año, y el día figura lleno
        ///   si alguna de sus comisiones llegó al límite.
        /// - Sin ellos, un día figura lleno si algún año + comisión llegó al límite.
        /// En todos los casos cada día trae el detalle en <c>porGrupo</c>. Una mesa de
        /// examen (sin comisión) cuenta en el grupo de cada comisión de su año.
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
                    e.IdExamen,
                    e.Fecha,
                    Curso = e.IdMateriaNavigation.Curso,
                    e.IdComision
                })
                .ToListAsync();

            var comisiones = await _context.Comisions
                .Select(c => new { c.IdComision, Comision = c.Comision1 })
                .ToListAsync();

            // Cada examen en el grupo (año + comisión) al que le ocupa lugar: un parcial
            // en el de su comisión; una mesa (sin comisión), en todas las de su año
            var lugaresOcupados = examenesDelMes
                .SelectMany(e => comisiones
                    .Where(c => e.IdComision == null || c.IdComision == e.IdComision)
                    .Select(c => new { e.IdExamen, e.Fecha, e.Curso, c.IdComision, c.Comision }))
                .ToList();

            // Si piden un año (y comisión) concreto, solo cuentan los exámenes de ese grupo
            if (idMateria.HasValue)
            {
                string? cursoFiltro = await _context.Materias
                    .Where(m => m.IdMateria == idMateria.Value)
                    .Select(m => m.Curso)
                    .FirstOrDefaultAsync();
                lugaresOcupados = lugaresOcupados
                    .Where(l => l.Curso == cursoFiltro && (!idComision.HasValue || l.IdComision == idComision.Value))
                    .ToList();
            }

            var estadoPorDia = lugaresOcupados
                .GroupBy(l => l.Fecha.Date)
                .Select(dia =>
                {
                    var porGrupo = dia
                        .GroupBy(l => new { l.Curso, l.IdComision, l.Comision })
                        .Select(g => new
                        {
                            Curso = g.Key.Curso,
                            IdComision = (int?)g.Key.IdComision,
                            Comision = g.Key.Comision,
                            CantidadExamenes = g.Count(),
                            LimiteAlcanzado = g.Count() >= limite
                        })
                        .ToList();

                    return new
                    {
                        Fecha = dia.Key,
                        // Exámenes distintos del día (una mesa cuenta una vez, aunque
                        // ocupe lugar en varias comisiones)
                        CantidadExamenes = dia.Select(l => l.IdExamen).Distinct().Count(),
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
                return BadRequest(new
                {
                    message = examen.IdComision == null
                        ? "No se puede reprogramar: alguna comisión de ese año ya tiene el máximo de exámenes permitido para ese día."
                        : "No se puede reprogramar: ese año y comisión ya tienen el máximo de exámenes permitido para ese día."
                });
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

                    // Las mesas de examen son de todas las comisiones del año
                    Comision = e.IdComision == null
                        ? TiposExamen.TodasLasComisiones
                        : e.IdComisionNavigation!.Comision1,
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

            if (!await _context.Docentes.AnyAsync(d => d.IdDocente == dto.IdDocente && d.IdUsuarioNavigation.EstadoUsuario))
                return BadRequest(new { message = "El docente seleccionado no existe o está dado de baja." });

            // 1. Verificamos que no exista ya una asignación para este examen y este docente
            //    (tampoco se puede volver a convocar a uno que ya rechazó esta mesa)
            bool asignacionExiste = await _context.MesasExamenes
                .AnyAsync(m => m.IdExamen == dto.IdExamen && m.IdDocente == dto.IdDocente);

            if (asignacionExiste)
            {
                return BadRequest(new { message = "El docente ya fue convocado a esta mesa de examen." });
            }

            // 2. Qué lugar del tribunal se cubre. Un lugar está libre si nadie lo tiene
            //    pendiente ni aceptado (por ejemplo, porque su docente lo rechazó).
            List<string> rolesOcupados = await _context.MesasExamenes
                .Where(m => m.IdExamen == dto.IdExamen
                    && (m.EstadoConfirmacion == MesaPendiente || m.EstadoConfirmacion == MesaAceptada))
                .Select(m => m.Rol)
                .ToListAsync();
            List<string> rolesLibres = RolesMesa.Todos.Except(rolesOcupados).ToList();

            string? rol = RolesMesa.Normalizar(dto.Rol);
            if (!string.IsNullOrWhiteSpace(dto.Rol) && rol == null)
                return BadRequest(new { message = $"El rol no es válido. Opciones: {string.Join(", ", RolesMesa.Todos)}." });

            if (rol == null)
            {
                if (rolesLibres.Count != 1)
                    return BadRequest(new
                    {
                        message = rolesLibres.Count == 0
                            ? "El tribunal de esta mesa ya está completo."
                            : $"Indicá qué lugar del tribunal se cubre: {string.Join(", ", rolesLibres)}."
                    });
                rol = rolesLibres[0];
            }
            else if (!rolesLibres.Contains(rol))
            {
                return BadRequest(new { message = $"El lugar de {rol} ya está cubierto en esta mesa." });
            }

            // Si la mesa había quedado libre (todos rechazaron), al reasignarla vuelve
            // a ocupar el día: tiene que haber lugar dentro del límite diario.
            bool yaOcupaElDia = await ExamenesQueOcupanElDia().AnyAsync(e => e.IdExamen == dto.IdExamen);
            if (!yaOcupaElDia && await SuperaLimiteDiarioAsync(examen.Fecha, examen.IdMateria, examen.IdComision, examen.IdExamen))
            {
                return BadRequest(new
                {
                    message = examen.IdComision == null
                        ? "No se puede reasignar la mesa: alguna comisión de ese año ya tiene el máximo de exámenes permitido para ese día."
                        : "No se puede reasignar la mesa: ese año y comisión ya tienen el máximo de exámenes permitido para ese día."
                });
            }

            // 3. Creamos la asignación
            MesaExamen nuevaAsignacion = new MesaExamen
            {
                IdExamen = dto.IdExamen,
                IdDocente = dto.IdDocente,
                Rol = rol,
                EstadoConfirmacion = MesaPendiente // Estado inicial automático
            };

            _context.MesasExamenes.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            await AvisarConvocatoriaAsync(dto.IdDocente, dto.IdExamen, rol);

            return Ok(new { message = $"Docente convocado como {rol}. Se le envió el aviso.", rol });
        }

        // Alta de una mesa de examen con su tribunal completo (presidente y dos vocales)

        [HttpPost("registrar-mesa")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> RegistrarMesa([FromBody] RegistrarMesaDto dto)
        {
            // El tribunal son tres docentes distintos, uno por lugar
            Dictionary<string, int> tribunal = new Dictionary<string, int>
            {
                [RolesMesa.Presidente] = dto.IdPresidente,
                [RolesMesa.Vocal1] = dto.IdVocal1,
                [RolesMesa.Vocal2] = dto.IdVocal2,
            };

            if (tribunal.Values.Any(id => id <= 0))
                return BadRequest(new { message = "Hay que indicar los tres docentes del tribunal: presidente, vocal 1 y vocal 2." });

            if (tribunal.Values.Distinct().Count() != tribunal.Count)
                return BadRequest(new { message = "Los tres docentes del tribunal tienen que ser distintos." });

            List<int> idsDocentes = tribunal.Values.ToList();
            int docentesValidos = await _context.Docentes
                .CountAsync(d => idsDocentes.Contains(d.IdDocente) && d.IdUsuarioNavigation.EstadoUsuario);
            if (docentesValidos != tribunal.Count)
            {
                return BadRequest(new { message = "Alguno de los docentes seleccionados no existe o está dado de baja." });
            }

            if (!await _context.Materias.AnyAsync(m => m.IdMateria == dto.IdMateria))
                return BadRequest(new { message = "La materia seleccionada no existe." });

            // La mesa es para todas las comisiones y siempre con el tipo "Mesa de Examen"
            // (lo que mande el front en IdComision e IdTipoExamen se ignora)
            int? idTipoMesa = await _context.TipoExamen
                .Where(t => t.TipoExamen == TiposExamen.MesaDeExamen)
                .Select(t => (int?)t.IdTipoExamen)
                .FirstOrDefaultAsync();
            if (idTipoMesa == null)
                return StatusCode(500, new { message = "Falta el tipo de examen \"Mesa de Examen\". Reiniciá la API para que se cargue." });

            // La mesa también cuenta para el límite de exámenes por día, en todas las
            // comisiones de ese año
            if (await SuperaLimiteDiarioAsync(dto.Fecha, dto.IdMateria, idComision: null))
            {
                return BadRequest(new { message = "No se puede registrar la mesa: alguna comisión de ese año ya tiene el máximo de exámenes permitido para ese día." });
            }

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso A: Insertamos los datos de la fecha y materia en Examenes
                Examene nuevaMesa = new Examene
                {
                    Fecha = dto.Fecha,
                    IdMateria = dto.IdMateria,
                    IdComision = null,
                    IdTipoExamen = idTipoMesa.Value
                };

                _context.Examenes.Add(nuevaMesa);
                await _context.SaveChangesAsync();


                // Paso B: convocamos a los tres integrantes del tribunal
                foreach (KeyValuePair<string, int> lugar in tribunal)
                {
                    _context.MesasExamenes.Add(new MesaExamen
                    {
                        IdExamen = nuevaMesa.IdExamen,
                        IdDocente = lugar.Value,
                        Rol = lugar.Key,
                        EstadoConfirmacion = MesaPendiente
                    });
                }
                await _context.SaveChangesAsync();

                // Confirmamos definitivamente todos los cambios en SQL Server
                await transaction.CommitAsync();

                foreach (KeyValuePair<string, int> lugar in tribunal)
                    await AvisarConvocatoriaAsync(lugar.Value, nuevaMesa.IdExamen, lugar.Key);

                return Ok(new
                {
                    message = "Mesa de examen registrada. Se envió el aviso a los tres docentes del tribunal; aparecerá en el calendario cuando los tres acepten.",
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

