using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.DTOs.Categories
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [Range(0, 300, ErrorMessage = "Вес должен быть от 0 до 300 кг")]
        public decimal MinWeight { get; set; }

        [Required]
        [Range(0, 300, ErrorMessage = "Вес должен быть от 0 до 300 кг")]
        public decimal MaxWeight { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Возраст должен быть от 0 до 100 лет")]
        public int MinAge { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Возраст должен быть от 0 до 100 лет")]
        public int MaxAge { get; set; }

        public string WeightRange => $"{MinWeight}-{MaxWeight} кг";
        public string AgeRange => $"{MinAge}-{MaxAge} лет";
        public string DisplayName => $"{WeightRange}, {AgeRange}";
    }
}
