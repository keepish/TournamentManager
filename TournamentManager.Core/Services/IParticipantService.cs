using TournamentManager.Core.DTOs.Participants;

namespace TournamentManager.Core.Services
{
    public interface IParticipantService
    {
        Task<List<ParticipantDto?>?> GetAllAsync();
        Task<ParticipantDto?> GetAsync(int id);
        Task<ParticipantDto> AddAsync(ParticipantDto participantDto);
        Task UpdateAsync(ParticipantDto participantDto);
        Task DeleteAsync(int id);
    }
}
