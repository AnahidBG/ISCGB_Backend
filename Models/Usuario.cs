using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool EstadoUsuario { get; set; }

    public string? TokenRecuperacion { get; set; }

    public DateTime? ExpiracionToken { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Dni { get; set; }

    public string? Telefono { get; set; }

    public string? TelefonoEmergencia { get; set; }

    public string? LugarNacimiento { get; set; }

    public string? ContactoEmergencia { get; set; }

    public string? Direccion { get; set; }

    public int? IdProvincia { get; set; }

    public virtual ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();

    public virtual ICollection<Docente> Docentes { get; set; } = new List<Docente>();

    public virtual Provincium? IdProvinciaNavigation { get; set; }

    public virtual ICollection<Justificativo> JustificativoIdUsuarioAuditorNavigations { get; set; } = new List<Justificativo>();

    public virtual ICollection<Justificativo> JustificativoIdUsuarioNavigations { get; set; } = new List<Justificativo>();

    public virtual ICollection<Legajo> LegajoIdUsuarioAuditorNavigations { get; set; } = new List<Legajo>();

    public virtual ICollection<Legajo> LegajoIdUsuarioNavigations { get; set; } = new List<Legajo>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
