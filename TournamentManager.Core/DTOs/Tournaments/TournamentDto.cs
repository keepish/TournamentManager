using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.DTOs.Tournaments
{
    public class TournamentDto
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
        [StringLength(15)]
        public string Status { get; set; } = "Предстоящий";

        [Required]
        public int OrganizerId { get; set; }
    }
}
