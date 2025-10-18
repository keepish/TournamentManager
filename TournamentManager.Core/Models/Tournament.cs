using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Address { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "Upcoming";

        public int OrganizerId { get; set; }

        public User Organizer { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
