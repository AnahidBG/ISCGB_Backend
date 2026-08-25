using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using AutoGestionAPI.DTOs;
using  AutoGestionAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TuDbContext _context; 
        private readonly IConfiguration _configuration;

        public AuthController(TuDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        

        //Función del login

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            
            Usuario? usuario = _context.Usuarios
                      .Include(u => u.UsuariosRoles)
                       .ThenInclude(ur => ur.IdRolNavigation)
                      .FirstOrDefault(u => u.Dni == request.Dni);

            if (usuario == null || usuario.EstadoUsuario == false)
            {
                return Unauthorized(new { message = "DNI no encontrado o cuenta inactiva." });
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

            return Ok(new { 
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
                Roles = listaRoles

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
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()), 
                new Claim("DNI", usuario.Dni)
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