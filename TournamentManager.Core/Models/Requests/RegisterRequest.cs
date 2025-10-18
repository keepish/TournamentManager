namespace TournamentManager.Core.Models.Requests
{
    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string? Email { get; set; }
        public string? Settlement { get; set; }
        public DateTime? Birthday { get; set; }
        public string? BeltLevel { get; set; }
    }
}
