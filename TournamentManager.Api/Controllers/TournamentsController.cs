using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.TournamentCategories;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournaments()
        {
            var tournaments = await context.Tournaments.ToListAsync();
            var tournamentsDto = tournaments.Select(t => t.ToDto()).ToList(); 
            return Ok(tournamentsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TournamentDto>> GetTournament(int id)
        {
            var tournament = await context.Tournaments.FindAsync(id);

            if (tournament is null)
                return NotFound();

            var tournamentDto = tournament.ToDto();

            return Ok(tournamentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournament(int id, TournamentDto tournamentDto)
        {
            if (id != tournamentDto.Id)
                return BadRequest();

            try
            {
                var tournament = new Tournament
                {
                    Id = tournamentDto.Id,
                    Name = tournamentDto.Name,
                    Description = tournamentDto.Description,
                    StartDate = tournamentDto.StartDate,
                    EndDate = tournamentDto.EndDate,
                    Address = tournamentDto.Address,
                    OrganizerId = tournamentDto.OrganizerId
                };

                context.Entry(tournament).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TournamentExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostTournament(TournamentDto tournamentDto)
        {
            var tournament = new Tournament
            {
                Id = tournamentDto.Id,
                Name = tournamentDto.Name,
                Description = tournamentDto.Description,
                StartDate = tournamentDto.StartDate,
                EndDate = tournamentDto.EndDate,
                Address = tournamentDto.Address,
                OrganizerId = tournamentDto.OrganizerId
            };

            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetTournament", new { id = tournamentDto.Id }, tournament);
        }

        [HttpPost("{id}/register-participants")]
        public async Task<ActionResult<TournamentRegistrationResultDto>> RegisterParticipants(int id)
        {
            var tournament = await context.Tournaments.FindAsync(id);
            if (tournament is null)
                return NotFound();

            // Load attached tournament categories with underlying category definitions
            var tournamentCategories = await context.TournamentCategories
                .Include(tc => tc.Category)
                .Where(tc => tc.TournamentId == id)
                .ToListAsync();

            if (!tournamentCategories.Any())
                return Ok(new TournamentRegistrationResultDto { TournamentId = id });

            // Preload all participants to evaluate eligibility
            var participants = await context.Participants.ToListAsync();
            var now = DateTime.UtcNow;
            var random = new Random();
            var result = new TournamentRegistrationResultDto { TournamentId = id };

            foreach (var tc in tournamentCategories)
            {
                var category = tc.Category;

                // Filter participants by age and weight (gender optional if future property exists)
                var eligible = participants
                    .Where(p =>
                        p.Weight >= category.MinWeight && p.Weight <= category.MaxWeight &&
                        GetAge(p.Birthday, now) >= category.MinAge && GetAge(p.Birthday, now) <= category.MaxAge)
                    .ToList();

                // Exclude already registered for this tournament category
                var existingParticipantIds = await context.ParticipantTournamentCategories
                    .Where(ptc => ptc.TournamentCategoryId == tc.Id)
                    .Select(ptc => ptc.ParticipantId)
                    .ToListAsync();

                eligible = eligible.Where(p => !existingParticipantIds.Contains(p.Id)).ToList();

                if (!eligible.Any())
                {
                    result.CategoryResults.Add(new TournamentRegistrationCategoryResultDto
                    {
                        TournamentCategoryId = tc.Id,
                        CategoryId = tc.CategoryId,
                        Gender = 0,
                        ParticipantsRegistered = 0,
                        MatchesCreated = 0
                    });
                    continue;
                }

                // Randomize order
                eligible = eligible.OrderBy(_ => random.Next()).ToList();

                // Register participants
                var participantTournamentCategories = eligible.Select(p => new ParticipantTournamentCategory
                {
                    TournamentCategoryId = tc.Id,
                    ParticipantId = p.Id
                }).ToList();

                context.ParticipantTournamentCategories.AddRange(participantTournamentCategories);
                await context.SaveChangesAsync();

                // Create matches (pair sequentially)
                var createdMatches = new List<Match>();
                for (int i = 0; i < participantTournamentCategories.Count; i += 2)
                {
                    var first = participantTournamentCategories[i];
                    ParticipantTournamentCategory? second = i + 1 < participantTournamentCategories.Count ? participantTournamentCategories[i + 1] : null;

                    var match = new Match
                    {
                        FirstParticipantId = first.Id,
                        SecondParticipantId = second?.Id,
                        FirstParticipantScore = 0,
                        SecondParticipantScore = 0
                    };
                    createdMatches.Add(match);
                }

                context.Matches.AddRange(createdMatches);
                await context.SaveChangesAsync();

                result.CategoryResults.Add(new TournamentRegistrationCategoryResultDto
                {
                    TournamentCategoryId = tc.Id,
                    CategoryId = tc.CategoryId,
                    Gender = 0,
                    ParticipantsRegistered = participantTournamentCategories.Count,
                    MatchesCreated = createdMatches.Count
                });
            }

            return Ok(result);
        }

        private static int GetAge(DateTime birthday, DateTime reference)
        {
            var age = reference.Year - birthday.Year;
            if (birthday.Date > reference.AddYears(-age)) age--;
            return age;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var tournament = await context.Tournaments.FindAsync(id);
            if (tournament is null)
                return NotFound();

            context.Tournaments.Remove(tournament);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool TournamentExists(int id)
        {
            return context.Tournaments.Any(e => e.Id == id);
        }

        [HttpGet("{id}/brackets")]
        public async Task<ActionResult<IEnumerable<CategoryBracketDto>>> GetBrackets(int id)
        {
            var tournament = await context.Tournaments.FindAsync(id);
            if (tournament is null)
                return NotFound();

            var tournamentCategories = await context.TournamentCategories
                .Include(tc => tc.Category)
                .Where(tc => tc.TournamentId == id)
                .ToListAsync();

            var result = new List<CategoryBracketDto>();

            foreach (var tc in tournamentCategories)
            {
                // get matches for participants in this tournament category
                var ptcIds = await context.ParticipantTournamentCategories
                    .Where(ptc => ptc.TournamentCategoryId == tc.Id)
                    .Select(ptc => ptc.Id)
                    .ToListAsync();

                var matches = await context.Matches
                    .Where(m => ptcIds.Contains(m.FirstParticipantId) || (m.SecondParticipantId != null && ptcIds.Contains(m.SecondParticipantId.Value)))
                    .ToListAsync();

                if (!matches.Any()) continue;

                var items = new List<BracketMatchItemDto>();

                foreach (var m in matches)
                {
                    var first = await context.ParticipantTournamentCategories
                        .Include(x => x.Participant)
                        .FirstOrDefaultAsync(x => x.Id == m.FirstParticipantId);
                    ParticipantTournamentCategory? second = null;
                    if (m.SecondParticipantId.HasValue)
                    {
                        second = await context.ParticipantTournamentCategories
                            .Include(x => x.Participant)
                            .FirstOrDefaultAsync(x => x.Id == m.SecondParticipantId);
                    }

                    items.Add(new BracketMatchItemDto
                    {
                        MatchId = m.Id,
                        FirstParticipantTournamentCategoryId = m.FirstParticipantId,
                        SecondParticipantTournamentCategoryId = m.SecondParticipantId,
                        FirstParticipantName = first?.Participant != null ? $"{first.Participant.Surname} {first.Participant.Name}" : "",
                        SecondParticipantName = second?.Participant != null ? $"{second.Participant.Surname} {second.Participant.Name}" : null,
                        FirstParticipantScore = m.FirstParticipantScore,
                        SecondParticipantScore = m.SecondParticipantScore
                    });
                }

                result.Add(new CategoryBracketDto
                {
                    TournamentCategoryId = tc.Id,
                    CategoryId = tc.CategoryId,
                    CategoryDisplay = $"{tc.Category.MinWeight}-{tc.Category.MaxWeight} кг, {tc.Category.MinAge}-{tc.Category.MaxAge} лет",
                    Matches = items
                });
            }

            return Ok(result);
        }
    }
}
