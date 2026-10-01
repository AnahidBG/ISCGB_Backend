using System.Net;
using System.Net.Mail;

namespace AutoGestionAPI.Services
{
    // 1. La interfaz ahora tiene el nombre exacto que busca tu controlador
    public interface IEmailService
    {
        Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion);
    }

    // 2. La clase implementa ese mismo método
    public class EmailService : IEmailService
    {
        public async Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("angelgabrielsilvajr@gmail.com", "zoswnnvqqpmyxnym"),
                EnableSsl = true,
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress("angelgabrielsilvajr@gmail.com", "Sistema Académico"),
                Subject = "Configura tu contraseña",
                Body = $"<h3>Hola {nombre}</h3><p>Hacé clic en el siguiente enlace para crear tu contraseña de acceso al sistema: <br><br> <a href='{urlConfiguracion}'>Configurar mi contraseña</a></p>",
                IsBodyHtml = true,
            };

            mensaje.To.Add(emailDestino);
            await smtpClient.SendMailAsync(mensaje);
        }
    }
}