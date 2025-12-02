using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Color { get; set; } = "#000000"; // Hex code
    }
}
