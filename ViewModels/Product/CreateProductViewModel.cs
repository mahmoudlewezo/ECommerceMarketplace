using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ECommerceMarketplace.ViewModels.Product
{
    public class CreateProductViewModel
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? Image { get; set; }
    }
}
