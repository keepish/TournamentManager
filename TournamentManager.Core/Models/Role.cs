using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<User> Users { get; set;} = new();
    }
}
