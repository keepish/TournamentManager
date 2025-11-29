using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Participants;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParticipantDto>>> GetParticipants()
        {
            var participants = await context.Participants.ToListAsync();
            var participantsDto = participants.Select(p => p.ToDto()).ToList();
            return Ok(participantsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParticipantDto>> GetParticipant(int id)
        {
            var participant = await context.Participants.FindAsync(id);

            if (participant is null)
                return NotFound();

            var participantDto = participant.ToDto();
            return Ok(participantDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutParticipant(int id, ParticipantDto participantDto)
        {
            if (id != participantDto.Id)
                return BadRequest();

            if (!ParticipantExists(id))
                return NotFound();

            try
            {
                var participant = new Participant
                {
                    Id = participantDto.Id,
                    Name = participantDto.Name,
                    Surname = participantDto.Surname,
                    Patronymic = participantDto.Patronymic,
                    Phone = participantDto.Phone,
                    Gender = participantDto.Gender,
                    Birthday = participantDto.Birthday,
                    Weight = participantDto.Weight
                };

                context.Entry(participant).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ParticipantDto>> PostParticipant(ParticipantDto participantDto)
        {
            var participant = new Participant
            {
                Id = participantDto.Id,
                Name = participantDto.Name,
                Surname = participantDto.Surname,
                Patronymic = participantDto.Patronymic,
                Phone = participantDto.Phone,
                Gender = participantDto.Gender,
                Birthday = participantDto.Birthday,
                Weight = participantDto.Weight
            };

            context.Participants.Add(participant);
            await context.SaveChangesAsync();

            var createdDto = participant.ToDto();
            return CreatedAtAction("GetParticipant", new { id = participantDto.Id }, createdDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant = await context.Participants.FindAsync(id);
            if (participant is null)
                return NotFound();

            context.Participants.Remove(participant);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipantExists(int id)
        {
            return context.Participants.Any(e => e.Id == id);
        }
    }
}
