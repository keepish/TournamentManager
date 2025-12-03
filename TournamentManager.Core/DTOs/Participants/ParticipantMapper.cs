using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.Participants
{
    public static class ParticipantMapper
    {
        public static ParticipantDto ToDto(this Participant participant)
        {
            if (participant is null)
                throw new ArgumentNullException(nameof(participant));

            return new ParticipantDto
            {
                Id = participant.Id,
                Name = participant.Name,
                Surname = participant.Surname,
                Patronymic = participant.Patronymic,
                Phone = participant.Phone,
                Gender = participant.Gender,
                Birthday = participant.Birthday,
                Weight = participant.Weight,
            };
        }
    }
}
