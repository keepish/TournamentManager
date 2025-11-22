using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core;
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
    }
}
