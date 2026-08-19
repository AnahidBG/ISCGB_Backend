using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class TipoTitulo
{
    public int IdTipoTitulo { get; set; }

    public string? NombreTitulo { get; set; }

    public virtual ICollection<DocenteTipoTitulo> DocenteTipoTitulos { get; set; } = new List<DocenteTipoTitulo>();
}
