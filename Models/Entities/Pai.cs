using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models.Entities;

public partial class Pai
{
    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Provincium> Provincia { get; set; } = new List<Provincium>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
