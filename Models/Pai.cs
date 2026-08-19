using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Pai
{
    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Provincium> Provincia { get; set; } = new List<Provincium>();
}
