using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.TournamentCategories;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentCategoriesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TournamentCategoryDto>>> GetTournamentCategories()
        {
            var tournamentCategories = await context.TournamentCategories.ToListAsync();
            var tournamentCategoriesDto = tournamentCategories.Select(tc => tc.ToDto()).ToList();
            return Ok(tournamentCategoriesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TournamentCategoryDto>> GetTournamentCategory(int id)
        {
            var tournamentCategory = await context.TournamentCategories.FindAsync(id);

            if (tournamentCategory is null)
                return NotFound();

            var tournamentCategoryDto = tournamentCategory.ToDto();
            return Ok(tournamentCategoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournamentCategory(int id, TournamentCategoryDto tournamentCategoryDto)
        {
            if (id != tournamentCategoryDto.Id)
                return BadRequest();

            if (!TournamentCategoryExists(id))
                return NotFound();

            try
            {
                var tournamentCategory = new TournamentCategory
                {
                    Id = tournamentCategoryDto.Id,
                    TournamentId = tournamentCategoryDto.TournamentId,
                    CategoryId = tournamentCategoryDto.CategoryId,
                    JudgeId = tournamentCategoryDto.JudgeId,
                    SitesNumber = tournamentCategoryDto.SitesNumber
                };

                context.Entry(tournamentCategory).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TournamentCategoryExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<TournamentCategoryDto>> PostTournamentCategory(TournamentCategoryDto tournamentCategoryDto)
        {
            var tournamentCategory = new TournamentCategory
            {
                Id = tournamentCategoryDto.Id,
                TournamentId = tournamentCategoryDto.TournamentId,
                CategoryId = tournamentCategoryDto.CategoryId,
                JudgeId = tournamentCategoryDto.JudgeId,
                SitesNumber = tournamentCategoryDto.SitesNumber
            };

            context.TournamentCategories.Add(tournamentCategory);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetTournamentCategory", new { id = tournamentCategoryDto.Id }, tournamentCategoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournamentCategory(int id)
        {
            var tournamentCategory = await context.TournamentCategories.FindAsync(id);
            if (tournamentCategory is null)
                return NotFound();

            context.TournamentCategories.Remove(tournamentCategory);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("tournament/{tournamentId}")]
        public async Task<ActionResult<IEnumerable<TournamentCategoryDto>>> GetByTournamentId(int tournamentId)
        {
            var tournamentCategory = await context.TournamentCategories
                .Where(tc => tc.TournamentId == tournamentId)
                .ToListAsync();

            var tournamentCategoryDto = tournamentCategory.Select(tc => tc.ToDto()).ToList();
            return Ok(tournamentCategoryDto);
        }

        [HttpGet("tournament/{tournamentId}/categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByTournamentId(int tournamentId)
        {
            var categories = await context.TournamentCategories
                .Where(tc => tc.TournamentId == tournamentId)
                .Include(tc => tc.Category)
                .Select(tc => tc.Category.ToDto())
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("tournament/{tournamentId}/category/{categoryId}")]
        public async Task<IActionResult> AddCategoryToTournament(int tournamentId, int categoryId, [FromBody] AttachCategoryRequest request)
        {
            var existingCategory = await context.TournamentCategories
                .FirstOrDefaultAsync(tc => tc.TournamentId == tournamentId && tc.CategoryId == categoryId);

            if (existingCategory != null)
                return Conflict("Категория уже прикреплена к турниру");

            var tournamentCategory = new TournamentCategory
            {
                TournamentId = tournamentId,
                CategoryId = categoryId,
                JudgeId = request.JudgeId,
                SitesNumber = request.SitesNumber
            };

            context.TournamentCategories.Add(tournamentCategory);
            await context.SaveChangesAsync();

            return StatusCode(201, tournamentCategory.ToDto());
        }

        [HttpDelete("tournament/{tournamentId}/category/{categoryId}")]
        public async Task<IActionResult> DetachCategoryFromTournament(int tournamentId, int categoryId)
        {
            var tournamentCategory = await context.TournamentCategories
                .FirstOrDefaultAsync(tc => tc.TournamentId == tournamentId && tc.CategoryId == categoryId);

            if (tournamentCategory is null)
                return NotFound();

            context.TournamentCategories.Remove(tournamentCategory);
            await context.SaveChangesAsync();

            return NoContent();
        }

        public class AttachCategoryRequest
        {
            public int JudgeId { get; set; }
            public int SitesNumber { get; set; }
        }

        private bool TournamentCategoryExists(int id)
        {
            return context.TournamentCategories.Any(e => e.Id == id);
        }
    }
}
