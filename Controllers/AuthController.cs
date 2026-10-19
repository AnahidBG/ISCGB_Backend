using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using AutoGestionAPI.DTOs;
using AutoGestionAPI.DTOs.Contrasena;
using AutoGestionAPI.Models;
using AutoGestionAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TuDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly IEmailService _emailService;

        public AuthController(TuDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // Endpoint 1 para el recupero de contraseña: Recibe la solicitud del DNI desde la pantalla de Angular
        [HttpPost("recuperar-contrasena")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Dni))
                return BadRequest(new { message = "El DNI es obligatorio." });

            Usuario? usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Dni == dto.Dni && u.EstadoUsuario == true);

            // Retornamos OK 
            if (usuario == null)
            {
                return Ok(new { message = "Si el DNI existe en el sistema, se enviará un correo con instrucciones." });
            }

            // Genera token seguro y fecha de expiración de 1 hora
            string tokenRecuperacion = Guid.NewGuid().ToString("N");
            usuario.TokenRecuperacion = tokenRecuperacion;
            usuario.ExpiracionToken = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            try
            {
                // URL de la app en Angular que abre la pantalla para colocar la nueva clave
                string urlRecuperacion = $"http://localhost:4200/restablecer-password?token={tokenRecuperacion}";
                await _emailService.EnviarLinkConfiguracionAsync(usuario.Email, usuario.Nombre, urlRecuperacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al intentar enviar el correo de recuperación.", error = ex.Message });
            }

            return Ok(new { message = "Si el DNI existe en el sistema, se enviará un correo con instrucciones." });
        }

        // Endpoint 2: Recibe la clave nueva y la actualiza en la BD
        [HttpPost("establecer-password")]
        [AllowAnonymous]
        public async Task<IActionResult> EstablecerPassword([FromBody] EstablecerPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NuevaPassword))
                return BadRequest(new { message = "El token y la nueva contraseña son obligatorios." });

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == dto.Token);

            if (usuario == null)
                return BadRequest(new { message = "El enlace es inválido." });

            if (usuario.ExpiracionToken < DateTime.UtcNow)
                return BadRequest(new { message = "El enlace ha expirado. Solicite uno nuevo." });

            // Actualizamos la contraseña con BCrypt y limpiamos los tokens consumidos
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            usuario.TokenRecuperacion = null;
            usuario.ExpiracionToken = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña configurada exitosamente. Ya puede iniciar sesión." });
        }


        //Función del login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            Usuario? usuario = _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.IdRolNavigation)

                .Include(u => u.Docentes)
                .FirstOrDefault(u => u.Dni == request.Dni);

            if (usuario == null || usuario.EstadoUsuario == false)
            {
                return Unauthorized(new { message = "DNI no encontrado o cuenta inactiva." });
            }

            if (usuario.PasswordHash == "PENDIENTE_CONFIGURACION")
            {
                return Unauthorized(new { message = "Debe configurar su contraseña por primera vez usando el enlace enviado a su correo electrónico." });
            }

            // Verificamos la contraseña encriptada (password_hash)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Contraseña incorrecta." });
            }

            var token = GenerarJwtToken(usuario);

            var listaRoles = usuario.UsuariosRoles.Select(ur => new
            {
                IdRol = ur.IdRol,
                NombreRol = ur.IdRolNavigation?.Rol
            }).ToList();

            bool esSuplente = usuario.Docentes.FirstOrDefault()?.DirectorSuplente ?? false;

            return Ok(new
            {
                Token = token,
                Usuario = usuario.Nombre + " " + usuario.Apellido,
                Estado_usuario = usuario.EstadoUsuario,
                DNI = usuario.Dni,
                Telefono = usuario.Telefono,
                TelefonoEmergencia = usuario.TelefonoEmergencia,
                Lugar_Nacimiento = usuario.LugarNacimiento,
                NombreContactoEmergencia = usuario.ContactoEmergencia,
                Direccion = usuario.Direccion,
                Email = usuario.Email,
                IdUsuario = usuario.IdUsuario,
                Roles = listaRoles,
                Afiliacion_Emergencia = usuario.AfiliacionEmergencia,
                Fecha_Nacimiento = usuario.FechaNac?.ToString("yyyy-MM-dd"),
                Cuil = usuario.Cuil,
                Genero = usuario.Genero,
                EsDirectorSuplente = esSuplente,
                Foto = usuario.FotoPerfil
            });
        }

        //Función de crear un usuario, esto es de prueba. No quedaría de esta forma.

        [HttpPost("crear-usuario-prueba")]
        public IActionResult CrearUsuarioPrueba([FromBody] LoginRequestDto request)
        {
            Usuario nuevoUsuario = new Usuario
            {
                Dni = request.Dni,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = "prueba@test.com",
                EstadoUsuario = true,
                UsuariosRoles = new List<UsuariosRole>
                {
                    new UsuariosRole { IdRol = 1 }
                }

            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            return Ok(new { message = "Usuario de prueba creado con éxito." });
        }

        private string GenerarJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim("DNI", usuario.Dni ?? "")
            };


            foreach (var rol in usuario.UsuariosRoles)
            {
                // Verificamos que el rol no sea nulo
                if (!string.IsNullOrEmpty(rol.IdRolNavigation?.Rol))
                {

                    claims.Add(new Claim(ClaimTypes.Role, rol.IdRolNavigation.Rol));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}