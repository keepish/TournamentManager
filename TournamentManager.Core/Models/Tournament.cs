using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class Tournament
{
    public int Id { get; set; }

    public int OrganizerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Address { get; set; } = null!;

    public virtual User Organizer { get; set; } = null!;

    public virtual ICollection<TournamentCategory> TournamentCategories { get; set; } = new List<TournamentCategory>();
}
