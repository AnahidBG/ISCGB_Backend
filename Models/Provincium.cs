using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Provincium
{
    public int IdProvincia { get; set; }

    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual Pai IdPaisNavigation { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
