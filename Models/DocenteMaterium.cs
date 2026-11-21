using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class DocenteMaterium
{
    public int IdDocenteMateria { get; set; }

    public int IdMateria { get; set; }

    public int IdDocente { get; set; }

    public int IdComision { get; set; }

    /// <summary>
    /// Durante una suplencia, el docente titular de la materia: la asignación pasó
    /// al suplente (<see cref="IdDocente"/>) y vuelve a este docente cuando se da de
    /// baja la suplencia. Es null en una asignación normal.
    /// </summary>
    public int? IdDocenteTitular { get; set; }

    public virtual Docente? IdDocenteTitularNavigation { get; set; }

    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;
}
