using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Role
{
    public int IdRol { get; set; }

    public string Rol { get; set; } = null!;

    public virtual ICollection<RolesTiposDocumento> RolesTiposDocumentos { get; set; } = new List<RolesTiposDocumento>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
