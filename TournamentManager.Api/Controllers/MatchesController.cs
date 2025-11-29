using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Matches;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches()
        {
            var matches = await context.Matches.ToListAsync();
            var matchesDto = matches.Select(m => m.ToDto()).ToList();
            return Ok(matchesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MatchDto>> GetMatch(int id)
        {
            var match = await context.Matches.FindAsync(id);

            if (match is null)
                return NotFound();

            var matchesDto = match.ToDto();
            return Ok(matchesDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatch(int id, MatchDto matchDto)
        {
            if (id != matchDto.Id)
                return BadRequest();

            if (!MatchExists(id))
                return NotFound();

            try
            {
                var match = new Match
                {
                    Id = matchDto.Id,
                    FirstParticipantId = matchDto.FirstParticipantId,
                    SecondParticipantId = matchDto.SecondParticipantId,
                    FirstParticipantScore = matchDto.FirstParticipantScore,
                    SecondParticipantScore = matchDto.SecondParticipantScore
                };

                context.Entry(match).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatchExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MatchDto>> PostMatch(MatchDto matchDto)
        {
            var match = new Match
            {
                Id = matchDto.Id,
                FirstParticipantId = matchDto.FirstParticipantId,
                SecondParticipantId = matchDto.SecondParticipantId,
                FirstParticipantScore = matchDto.FirstParticipantScore,
                SecondParticipantScore = matchDto.SecondParticipantScore
            };

            context.Matches.Add(match);
            await context.SaveChangesAsync();

            var createdDto = match.ToDto();
            return CreatedAtAction("GetMatch", new { id = matchDto.Id }, matchDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await context.Matches.FindAsync(id);
            if (match is null)
                return NotFound();

            context.Matches.Remove(match);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool MatchExists(int id)
        {
            return context.Matches.Any(e => e.Id == id);
        }
    }
}
