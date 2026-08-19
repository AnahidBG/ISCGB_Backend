using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Examene
{
    public int IdExamen { get; set; }

    public int IdMateria { get; set; }

    public int IdComision { get; set; }

    public int IdTipoExamen { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;

    public virtual TipoExaman IdTipoExamenNavigation { get; set; } = null!;

    public virtual ICollection<Docente> IdDocentes { get; set; } = new List<Docente>();
}
