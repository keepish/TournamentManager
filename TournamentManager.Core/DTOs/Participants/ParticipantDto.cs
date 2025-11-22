namespace TournamentManager.Core.DTOs.Participants
{
    public class ParticipantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string? Phone { get; set; }
        public ulong Gender { get; set; }
        public DateTime Birthday { get; set; }
        public decimal Weight { get; set; }

        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
        public string GenderDisplay => Gender == 1 ? "Мужской" : "Женский";
        public int Age => DateTime.Now.Year - Birthday.Year; 
    }
}
