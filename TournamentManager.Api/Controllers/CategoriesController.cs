using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await context.Categories.ToListAsync();
            var categoriesDto = categories.Select(c => c.ToDto()).ToList();
            return Ok(categoriesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await context.Categories.FindAsync(id);

            if (category is null)
                return NotFound();

            var categoryDto = category.ToDto();
            return Ok(categoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
                return BadRequest();

            if (!CategoryExists(id))
                return NotFound();

            try
            {
                var category = new Category
                {
                    Id = categoryDto.Id,
                    MinWeight = categoryDto.MinWeight,
                    MaxWeight = categoryDto.MaxWeight,
                    MinAge = categoryDto.MinAge,
                    MaxAge = categoryDto.MaxAge
                };

                context.Entry(category).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                    return NotFound();
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Id = categoryDto.Id,
                MinWeight = categoryDto.MinWeight,
                MaxWeight = categoryDto.MaxWeight,
                MinAge = categoryDto.MinAge,
                MaxAge = categoryDto.MaxAge
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var createdDto = category.ToDto();
            return CreatedAtAction("GetCategory", new { id = categoryDto.Id }, createdDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category is null)
                return NotFound();

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return context.Categories.Any(e => e.Id == id);
        }
    }
}
