using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.ParticipantTournamentCategories;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantTournamentCategoriesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParticipantTournamentCategoryDto>>> GetParticipantTournamentCategories()
        {
            var participantTournamentCategories = await context.ParticipantTournamentCategories.ToListAsync();
            var participantTournamentCategoriesDto = participantTournamentCategories.Select(ptc => ptc.ToDto()).ToList();
            return Ok(participantTournamentCategoriesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParticipantTournamentCategoryDto>> GetParticipantTournamentCategory(int id)
        {
            var participantTournamentCategory = await context.ParticipantTournamentCategories.FindAsync(id);

            if (participantTournamentCategory is null)
                return NotFound();

            var participantTournamentCategoryDto = participantTournamentCategory.ToDto();
            return Ok(participantTournamentCategoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutParticipantTournamentCategory(int id, ParticipantTournamentCategoryDto participantTournamentCategoryDto)
        {
            if (id != participantTournamentCategoryDto.Id)
                return BadRequest();

            if (!ParticipantTournamentCategoryExists(id))
                return NotFound();

            try
            {
                var participantTournamentCategory = new ParticipantTournamentCategory
                {
                    Id = participantTournamentCategoryDto.Id,
                    TournamentCategoryId = participantTournamentCategoryDto.TournamentCategoryId,
                    ParticipantId = participantTournamentCategoryDto.ParticipantId
                };

                context.Entry(participantTournamentCategory).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantTournamentCategoryExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ParticipantTournamentCategoryDto>> PostParticipantTournamentCategory(ParticipantTournamentCategoryDto participantTournamentCategoryDto)
        {
            var participantTournamentCategory = new ParticipantTournamentCategory
            {
                Id = participantTournamentCategoryDto.Id,
                TournamentCategoryId = participantTournamentCategoryDto.TournamentCategoryId,
                ParticipantId = participantTournamentCategoryDto.ParticipantId
            };

            context.ParticipantTournamentCategories.Add(participantTournamentCategory);
            await context.SaveChangesAsync();

            var createdDto = participantTournamentCategory.ToDto();
            return CreatedAtAction("GetParticipantTournamentCategory", new { id = participantTournamentCategory.Id }, createdDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipantTournamentCategory(int id)
        {
            var participantTournamentCategory = await context.ParticipantTournamentCategories.FindAsync(id);
            if (participantTournamentCategory is null)
                return NotFound();

            context.ParticipantTournamentCategories.Remove(participantTournamentCategory);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipantTournamentCategoryExists(int id)
        {
            return context.ParticipantTournamentCategories.Any(e => e.Id == id);
        }
    }
}
