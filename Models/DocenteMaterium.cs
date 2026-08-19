using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class DocenteMaterium
{
    public int IdDocenteMateria { get; set; }

    public int IdMateria { get; set; }

    public int IdDocente { get; set; }

    public int IdComision { get; set; }

    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;
}
