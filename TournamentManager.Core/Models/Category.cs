using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class Category
{
    public int Id { get; set; }

    public decimal MinWeight { get; set; }

    public decimal MaxWeight { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public virtual ICollection<TournamentCategory> TournamentCategories { get; set; } = new List<TournamentCategory>();
}
