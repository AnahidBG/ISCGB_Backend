using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.Services;

namespace AutoGestionAPI.Workers
{
    public class NotificadorFaltantesWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificadorFaltantesWorker> _logger;

        public NotificadorFaltantesWorker(IServiceProvider serviceProvider, ILogger<NotificadorFaltantesWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Motor de notificaciones automáticas iniciado (Escenario B).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<TuDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var config = await context.ConfiguracionesSistema.FirstOrDefaultAsync(stoppingToken);
                        int diasFrecuencia = config?.FrecuenciaNotificacionDias ?? 7;
                        var fechaLimite = DateTime.UtcNow.AddDays(-diasFrecuencia);

                        // 1. Buscamos TODOS los tipos de documentos que existen en el sistema
                        var todosLosTiposDocumentos = await context.TiposDocumentos.ToListAsync(stoppingToken);

                        // 2. Buscamos a los usuarios a los que ya se les cumplió el plazo para ser notificados
                        var usuarios = await context.Usuarios
                            .Include(u => u.LegajoIdUsuarioNavigations)
                            .Where(u => u.FechaUltimaNotificacion == null || u.FechaUltimaNotificacion < fechaLimite)
                            .ToListAsync(stoppingToken);

                        int usuariosNotificados = 0;

                        foreach (var usuario in usuarios)
                        {
                            // 3. Obtenemos los IDs de los documentos que el usuario YA subió (estén Pendientes o Aprobados)
                            var idsSubidos = usuario.LegajoIdUsuarioNavigations
                                .Select(l => l.IdTipoDoc)
                                .ToList();

                            // 4. Comparamos: ¿Qué documentos de la lista general NO están en la lista de subidos del usuario?
                            var documentosFaltantes = todosLosTiposDocumentos
                                .Where(td => !idsSubidos.Contains(td.IdTipoDoc))
                                .Select(td => td.NombreDocumento) // Usamos NombreDocumento tal cual está en tu controlador
                                .ToList();

                            // 5. Si le falta al menos un documento, le mandamos el correo
                            if (documentosFaltantes.Any())
                            {
                                try
                                {
                                    await emailService.EnviarAvisoFaltantesAsync(usuario.Email, usuario.Nombre, documentosFaltantes);
                                    usuario.FechaUltimaNotificacion = DateTime.UtcNow;
                                    usuariosNotificados++;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"No se pudo enviar el correo a {usuario.Email}");
                                }
                            }
                        }

                        // 6. Guardamos los cambios en la base de datos (las nuevas fechas de notificación)
                        if (usuariosNotificados > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Se enviaron {usuariosNotificados} avisos de documentación faltante.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al ejecutar el motor de notificaciones.");
                }

                // Para probarlo AHORA MISMO, descomentá la de 1 minuto y comentá la de 24 horas:
                // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}