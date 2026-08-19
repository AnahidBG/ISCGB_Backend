using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class AlumnoMaterium
{
    public int IdAlumnoMateria { get; set; }

    public int IdMateria { get; set; }

    public int IdAlumno { get; set; }

    public int IdComision { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;
}
