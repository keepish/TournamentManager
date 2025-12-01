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

            // Determine winner by score/byes
            int winnerPtcId;
            if (match.SecondParticipantId is null)
            {
                winnerPtcId = match.FirstParticipantId; // bye or placeholder progressed
            }
            else
            {
                winnerPtcId = match.FirstParticipantScore >= match.SecondParticipantScore
                    ? match.FirstParticipantId
                    : match.SecondParticipantId.Value;
            }

            // Determine tournament category by ParticipantTournamentCategory
            var winnerPtc = await context.ParticipantTournamentCategories
                .Include(ptc => ptc.TournamentCategory)
                .FirstOrDefaultAsync(ptc => ptc.Id == winnerPtcId);
            if (winnerPtc is null)
                return BadRequest("Winner participant not found");

            var tcId = winnerPtc.TournamentCategoryId;

            // Build seeding for this category (stable order by PTC Id)
            var seedPtcs = await context.ParticipantTournamentCategories
                .Where(ptc => ptc.TournamentCategoryId == tcId)
                .OrderBy(ptc => ptc.Id)
                .Select(ptc => ptc.Id)
                .ToListAsync();

            if (seedPtcs.Count == 0)
                return Ok(match.ToDto());

            int IndexOf(int ptcId) => seedPtcs.FindIndex(x => x == ptcId);

            int iA = IndexOf(match.FirstParticipantId);
            int iB = match.SecondParticipantId.HasValue ? IndexOf(match.SecondParticipantId.Value) : iA; // if bye, use same index

            // Compute current round r from indices (minimal block containing both)
            int diff = Math.Abs(iA - iB);
            int r = 1;
            int block = 1;
            while (block <= diff)
            {
                block <<= 1;
                r++;
            }
            // r is 1 for adjacent pair, 2 for block size 4, etc.

            // Compute parent group range for next round
            int childBlock = 1 << (r - 1); // size for this match
            int parentBlock = childBlock << 1;
            int minIndex = Math.Min(iA, iB);
            int parentStart = (minIndex / parentBlock) * parentBlock;
            bool isLeftChild = (minIndex - parentStart) < childBlock;

            // Try to find existing placeholder parent match in this group (with one participant)
            var candidates = await context.Matches
                .Where(m => m.SecondParticipantId == null)
                .Join(context.ParticipantTournamentCategories,
                      m => m.FirstParticipantId,
                      ptc => ptc.Id,
                      (m, ptc) => new { m, ptc })
                .Where(x => x.ptc.TournamentCategoryId == tcId)
                .Select(x => x.m)
                .ToListAsync();

            Match? parent = null;
            foreach (var cand in candidates)
            {
                var idx = IndexOf(cand.FirstParticipantId);
                if (idx >= parentStart && idx < parentStart + parentBlock)
                {
                    parent = cand;
                    break;
                }
            }

            if (parent != null)
            {
                if (parent.SecondParticipantId == null)
                {
                    // Fill the other slot
                    if (parent.FirstParticipantId != winnerPtcId)
                        parent.SecondParticipantId = winnerPtcId;
                    else
                        return Ok(parent.ToDto());

                    context.Entry(parent).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    return Ok(parent.ToDto());
                }
            }

            // Otherwise create a new next-round match with winner waiting
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
