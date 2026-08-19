using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class TipoExaman
{
    public int IdTipoExamen { get; set; }

    public string TipoExamen { get; set; } = null!;

    public virtual ICollection<Examene> Examenes { get; set; } = new List<Examene>();
}
