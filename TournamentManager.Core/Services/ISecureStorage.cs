namespace TournamentManager.Core.Services
{
    public interface ISecureStorage
    {
        void Save(string key, string value);
        string Load(string key);
        void Remove(string key);
        bool Contains(string key);
    }
}
