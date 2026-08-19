using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class ReconocimientoSabere
{
    public int IdSolicitud { get; set; }

    public int IdMateria { get; set; }

    public int IdAlumno { get; set; }

    public int? IdDocente { get; set; }

    public string? Comentario { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual Docente? IdDocenteNavigation { get; set; }

    public virtual Materia IdMateriaNavigation { get; set; } = null!;
}
