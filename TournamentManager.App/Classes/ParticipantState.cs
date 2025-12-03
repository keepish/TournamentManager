using System.Collections.ObjectModel;
using TournamentManager.Core.DTOs.Participants;

namespace TournamentManager.Client.Classes
{
    public static class ParticipantState
    {
        private static readonly Dictionary<int, ObservableCollection<ParticipantDto>> _tournamentParticipants = new();

        public static void SetParticipants(int tournamentId, ObservableCollection<ParticipantDto> participants)
        {
            _tournamentParticipants[tournamentId] = participants;
        }

        public static ObservableCollection<ParticipantDto> GetParticipants(int tournamentId)
        {
            if (_tournamentParticipants.ContainsKey(tournamentId))
                return _tournamentParticipants[tournamentId];

            var participants = new ObservableCollection<ParticipantDto>();
            SetParticipants(tournamentId, participants);
            return participants;
        }

        public static void ClearParticipants(int tournamentId)
        {
            if (_tournamentParticipants.ContainsKey(tournamentId))
                _tournamentParticipants.Remove(tournamentId);
        }
    }
}
