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

    /// <summary>
    /// Docente que cargó el examen desde el calendario. Es null en las mesas que
    /// registra Dirección/Secretaría y en los exámenes anteriores a esta columna.
    /// Un docente solo puede modificar o eliminar los exámenes que cargó él.
    /// </summary>
    public int? IdDocenteCreador { get; set; }

    public virtual Docente? IdDocenteCreadorNavigation { get; set; }

    public virtual Comision IdComisionNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;

    public virtual TipoExaman IdTipoExamenNavigation { get; set; } = null!;

    public virtual ICollection<Docente> IdDocentes { get; set; } = new List<Docente>();
}
