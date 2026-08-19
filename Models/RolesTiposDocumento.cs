using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class RolesTiposDocumento
{
    public int IdRolesTiposDocumentos { get; set; }

    public int IdRol { get; set; }

    public int IdTipoDoc { get; set; }

    public bool Obligatorio { get; set; }

    public bool Anual { get; set; }

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual TiposDocumento IdTipoDocNavigation { get; set; } = null!;
}
