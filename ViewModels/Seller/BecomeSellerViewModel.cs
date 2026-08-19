using System.ComponentModel.DataAnnotations;

namespace ECommerceMarketplace.ViewModels.Seller
{
    public class BecomeSellerViewModel
    {
        [Required(ErrorMessage = "Please provide a reason.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage = "Reason must be between 10 and 500 characters.")]
        [Display(Name = "Why do you want to become a seller?")]
        public string Reason { get; set; } = string.Empty;
    }
}