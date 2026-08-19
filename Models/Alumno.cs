using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Alumno
{
    public int IdAlumno { get; set; }

    public int IdUsuario { get; set; }

    public string? Cohorte { get; set; }

    public string? EstadoAcademico { get; set; }

    public virtual ICollection<AlumnoMaterium> AlumnoMateria { get; set; } = new List<AlumnoMaterium>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<ReconocimientoSabere> ReconocimientoSaberes { get; set; } = new List<ReconocimientoSabere>();
}
