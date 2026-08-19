using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class DocenteTipoTitulo
{
    public int IdDocenteTipoTitulo { get; set; }

    public int IdDocente { get; set; }

    public int IdTipoTitulo { get; set; }

    public string? Institucion { get; set; }

    public DateTime? FechaEgreso { get; set; }

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual TipoTitulo IdTipoTituloNavigation { get; set; } = null!;
}
