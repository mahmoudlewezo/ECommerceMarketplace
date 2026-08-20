using System.ComponentModel.DataAnnotations;

namespace ECommerceMarketplace.ViewModels.Category
{
    public class CreateCategoryViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
