namespace TournamentManager.Client.Classes
{
    public static class GenderOptions
    {
        public static List<GenderOption> Values => new List<GenderOption>
        {
            new GenderOption { Value = 1, Display = "Мужской" },
            new GenderOption { Value = 0, Display = "Женский" }
        };
    }

    public class GenderOption
    {
        public ulong Value { get; set; }
        public string Display { get; set; } = string.Empty;
    }
}
