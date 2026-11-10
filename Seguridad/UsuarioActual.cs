using System.Security.Claims;

namespace AutoGestionAPI.Seguridad
{
    /// <summary>
    /// Ayudas para leer quién hace el pedido a partir del token JWT.
    /// </summary>
    public static class UsuarioActual
    {
        /// <summary>Roles con permisos administrativos sobre datos de otras personas.</summary>
        public const string RolesAdministrativos = "Director,Secretario";

        /// <summary>Id del usuario logueado (claim NameIdentifier), o null si no viene.</summary>
        public static int? ObtenerIdUsuario(this ClaimsPrincipal usuario)
        {
            string? valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : null;
        }

        /// <summary>True si el usuario es Director o Secretario.</summary>
        public static bool EsAdministrativo(this ClaimsPrincipal usuario)
        {
            return usuario.IsInRole("Director") || usuario.IsInRole("Secretario");
        }

        /// <summary>
        /// True si el usuario logueado es el dueño del recurso (mismo id) o es
        /// administrativo. Se usa para evitar que alguien vea o cargue datos de
        /// otra persona cambiando el id en la URL o en el body.
        /// </summary>
        public static bool PuedeAccederA(this ClaimsPrincipal usuario, int idUsuarioRecurso)
        {
            return usuario.EsAdministrativo() || usuario.ObtenerIdUsuario() == idUsuarioRecurso;
        }
    }
}
