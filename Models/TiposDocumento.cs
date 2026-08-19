using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class TiposDocumento
{
    public int IdTipoDoc { get; set; }

    public string NombreDocumento { get; set; } = null!;

    public virtual ICollection<Legajo> Legajos { get; set; } = new List<Legajo>();

    public virtual ICollection<RolesTiposDocumento> RolesTiposDocumentos { get; set; } = new List<RolesTiposDocumento>();
}
