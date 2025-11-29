using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core;
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

        private bool TournamentCategoryExists(int id)
        {
            return context.TournamentCategories.Any(e => e.Id == id);
        }
    }
}
