using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.Services;

namespace AutoGestionAPI.Workers
{
    /// <summary>
    /// Aviso automático de documentación faltante.
    ///
    /// Corre UNA VEZ POR DÍA a una hora fija (por defecto 08:00, hora argentina),
    /// configurable en appsettings.json → "Notificaciones". En cada pasada, a cada
    /// usuario le manda el mail solo si pasaron los días configurados desde su
    /// último aviso (frecuencia que ajustan Dirección/Secretaría desde el sistema).
    ///
    /// Si la API arranca después de la hora de envío, hace una pasada en ese
    /// momento para no perder el día.
    /// </summary>
    public class NotificadorFaltantesWorker : BackgroundService
    {
        // Margen para que "cada N días" no se corra un día por unos segundos de
        // diferencia entre la pasada de hoy y la del último aviso.
        private static readonly TimeSpan Tolerancia = TimeSpan.FromHours(1);

        // Usamos IServiceScopeFactory porque este servicio es singleton y el
        // DbContext es scoped: en cada pasada se crea un scope nuevo.
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificadorFaltantesWorker> _logger;
        private readonly TimeSpan _horaEnvio;
        private readonly TimeZoneInfo _zonaHoraria;

        public NotificadorFaltantesWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificadorFaltantesWorker> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _horaEnvio = LeerHoraEnvio(configuration["Notificaciones:HoraEnvio"]);
            _zonaHoraria = LeerZonaHoraria(configuration["Notificaciones:ZonaHoraria"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Avisos de documentación faltante: una vez por día a las {Hora} ({Zona}).",
                _horaEnvio.ToString(@"hh\:mm"), _zonaHoraria.Id);

            // Si arrancamos después de la hora de envío, hacemos la pasada de hoy ahora.
            if (AhoraLocal().TimeOfDay >= _horaEnvio)
                await EnviarAvisosAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan espera = TiempoHastaProximoEnvio();
                _logger.LogInformation("Próximo envío de avisos en {Espera}.", espera.ToString(@"hh\:mm\:ss"));

                try
                {
                    await Task.Delay(espera, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break; // la API se está apagando
                }

                await EnviarAvisosAsync(stoppingToken);
            }
        }

        private async Task EnviarAvisosAsync(CancellationToken stoppingToken)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                TuDbContext context = scope.ServiceProvider.GetRequiredService<TuDbContext>();
                IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                ConfiguracionSistema? config = await context.ConfiguracionesSistema.OrderBy(c => c.IdConfiguracion).FirstOrDefaultAsync(stoppingToken);
                int diasFrecuencia = config?.FrecuenciaNotificacionDias ?? 7;
                DateTime fechaLimite = DateTime.UtcNow.AddDays(-diasFrecuencia).Add(Tolerancia);

                // Traemos los catálogos para no ir a la base de datos a cada rato
                List<TiposDocumento> todosLosTiposDocumentos = await context.TiposDocumentos.ToListAsync(stoppingToken);

                // Traemos SOLAMENTE las reglas donde Obligatorio es verdadero
                List<RolesTiposDocumento> reglasDocumentos = await context.RolesTiposDocumentos
                    .Where(r => r.Obligatorio == true)
                    .ToListAsync(stoppingToken);

                // Usuarios activos a los que ya les toca un aviso.
                // Los dados de baja (EstadoUsuario = false) no reciben avisos.
                List<Usuario> usuarios = await context.Usuarios
                    .Include(u => u.LegajoIdUsuarioNavigations)
                    .Include(u => u.UsuariosRoles)
                    .Where(u => u.EstadoUsuario)
                    .Where(u => u.FechaUltimaNotificacion == null || u.FechaUltimaNotificacion < fechaLimite)
                    .AsSplitQuery()
                    .ToListAsync(stoppingToken);

                int usuariosNotificados = 0;

                foreach (Usuario usuario in usuarios)
                {
                    List<int> idsRolesUsuario = usuario.UsuariosRoles.Select(ur => ur.IdRol).ToList();

                    // Misma regla que la pantalla de faltantes: no cuentan los
                    // rechazados, y los documentos anuales tienen que ser de este año.
                    List<int> idsFaltantes = DocumentacionService.CalcularFaltantes(
                            reglasDocumentos.Where(r => idsRolesUsuario.Contains(r.IdRol)),
                            usuario.LegajoIdUsuarioNavigations,
                            AhoraLocal())
                        .Select(grupo => grupo.Key)
                        .ToList();

                    if (!idsFaltantes.Any())
                        continue;

                    List<string> nombresDocumentosFaltantes = todosLosTiposDocumentos
                        .Where(td => idsFaltantes.Contains(td.IdTipoDoc))
                        .Select(td => td.NombreDocumento)
                        .ToList();

                    try
                    {
                        await emailService.EnviarAvisoFaltantesAsync(usuario.Email, usuario.Nombre ?? "Usuario", nombresDocumentosFaltantes);
                        usuario.FechaUltimaNotificacion = DateTime.UtcNow;
                        usuariosNotificados++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "No se pudo enviar el correo a {Email}", usuario.Email);
                    }
                }

                if (usuariosNotificados > 0)
                    await context.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Pasada de avisos terminada: {Cantidad} avisos enviados.", usuariosNotificados);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Ocurrió un error al ejecutar el motor de notificaciones.");
            }
        }

        private DateTime AhoraLocal()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zonaHoraria);
        }

        private TimeSpan TiempoHastaProximoEnvio()
        {
            DateTime ahora = AhoraLocal();
            DateTime proximo = ahora.Date.Add(_horaEnvio);
            if (proximo <= ahora)
                proximo = proximo.AddDays(1);

            return proximo - ahora;
        }

        private TimeSpan LeerHoraEnvio(string? valor)
        {
            if (TimeSpan.TryParse(valor, out TimeSpan hora) && hora >= TimeSpan.Zero && hora < TimeSpan.FromDays(1))
                return hora;

            if (!string.IsNullOrWhiteSpace(valor))
                _logger.LogWarning("'Notificaciones:HoraEnvio' = '{Valor}' no es una hora válida (HH:mm). Se usa 08:00.", valor);

            return new TimeSpan(8, 0, 0);
        }

        private TimeZoneInfo LeerZonaHoraria(string? id)
        {
            // Por defecto, hora argentina. Importa en Docker/servidores, que suelen estar en UTC.
            string zona = string.IsNullOrWhiteSpace(id) ? "America/Argentina/Buenos_Aires" : id;
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(zona);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                _logger.LogWarning("No se encontró la zona horaria '{Zona}'. Se usa la del servidor.", zona);
                return TimeZoneInfo.Local;
            }
        }
    }
}
