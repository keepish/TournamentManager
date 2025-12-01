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

        [HttpPost("{id}/advance")]
        public async Task<ActionResult<MatchDto>> AdvanceWinner(int id)
        {
            var match = await context.Matches.FindAsync(id);
            if (match is null)
                return NotFound();

            // Determine winner by score
            int winnerPtcId;
            if (match.SecondParticipantId is null)
            {
                winnerPtcId = match.FirstParticipantId;
            }
            else
            {
                winnerPtcId = match.FirstParticipantScore >= match.SecondParticipantScore
                    ? match.FirstParticipantId
                    : match.SecondParticipantId.Value;
            }

            // Find tournament category via ParticipantTournamentCategory
            var winnerPtc = await context.ParticipantTournamentCategories
                .Include(ptc => ptc.TournamentCategory)
                .FirstOrDefaultAsync(ptc => ptc.Id == winnerPtcId);
            if (winnerPtc is null)
                return BadRequest("Winner participant not found");

            var tcId = winnerPtc.TournamentCategoryId;

            // Try to find an existing next-round placeholder with only one participant
            var placeholder = await context.Matches
                .Where(m => m.SecondParticipantId == null)
                .Where(m => m.FirstParticipantId != winnerPtcId)
                .Join(context.ParticipantTournamentCategories,
                      m => m.FirstParticipantId,
                      ptc => ptc.Id,
                      (m, ptc) => new { m, ptc })
                .Where(x => x.ptc.TournamentCategoryId == tcId)
                .Select(x => x.m)
                .FirstOrDefaultAsync();

            if (placeholder != null)
            {
                placeholder.SecondParticipantId = winnerPtcId;
                context.Entry(placeholder).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(placeholder.ToDto());
            }

            // Otherwise create a new match with winner waiting for opponent
            var next = new Match
            {
                FirstParticipantId = winnerPtcId,
                SecondParticipantId = null,
                FirstParticipantScore = 0,
                SecondParticipantScore = 0
            };
            context.Matches.Add(next);
            await context.SaveChangesAsync();
            return Ok(next.ToDto());
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
