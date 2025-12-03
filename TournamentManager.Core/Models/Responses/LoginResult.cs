namespace TournamentManager.Core.Models.Responses
{
    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsOrganizer { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
