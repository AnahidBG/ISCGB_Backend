using System.Net;
using System.Net.Mail;

namespace AutoGestionAPI.Services
{
    // 1. LA INTERFAZ: Acá van solo las firmas (terminan con punto y coma)
    public interface IEmailService
    {
        Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion);

        // Esta es la firma nueva que faltaba acá arriba
        Task EnviarAvisoFaltantesAsync(string email, string nombre, List<string> documentosFaltantes);
    }

    // 2. LA CLASE: Acá va el código real de cada función (terminan con llaves { })
    public class EmailService : IEmailService
    {
        public async Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("notificacionesiscgb@gmail.com", "ylqteryqzxovutgs"),
                EnableSsl = true,
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress("notificacionesiscgb@gmail.com", "Sistema Académico"),
                Subject = "Configura tu contraseña",
                Body = $"<h3>Hola {nombre}</h3><p>Hacé clic en el siguiente enlace para crear tu contraseña de acceso al sistema: <br><br> <a href='{urlConfiguracion}'>Configurar mi contraseña</a></p>",
                IsBodyHtml = true,
            };

            mensaje.To.Add(emailDestino);
            await smtpClient.SendMailAsync(mensaje);
        }

        // Este es el método nuevo con su "cuerpo" de código
        public async Task EnviarAvisoFaltantesAsync(string email, string nombre, List<string> documentosFaltantes)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("notificacionesiscgb@gmail.com", "ylqteryqzxovutgs"),
                EnableSsl = true,
            };

            // Armamos la lista de viñetas HTML con los documentos
            string listaHtml = "";
            foreach (var doc in documentosFaltantes)
            {
                listaHtml += $"<li><strong>{doc}</strong></li>";
            }

            var mensaje = new MailMessage
            {
                From = new MailAddress("notificacionesiscgb@gmail.com", "Sistema Académico"),
                Subject = "Aviso de Documentación Faltante",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h3>Hola {nombre},</h3>
                        <p>Te contactamos desde la administración para recordarte que tenés documentación pendiente de entrega en tu legajo.</p>
                        <p>Actualmente adeudás los siguientes documentos:</p>
                        <ul>
                            {listaHtml}
                        </ul>
                        <p>Por favor, regularizá esta situación a la brevedad presentándolos en secretaría.</p>
                        <p>Saludos cordiales.</p>
                    </div>",
                IsBodyHtml = true,
            };

            mensaje.To.Add(email);
            await smtpClient.SendMailAsync(mensaje);
        }
    }
}