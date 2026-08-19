using System.ComponentModel.DataAnnotations;

namespace ECommerceMarketplace.Models
{
    public class SellerRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public SellerRequestStatus Status { get; set; } = SellerRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedById { get; set; }

        public ApplicationUser? ReviewedBy { get; set; }
    }

    public enum SellerRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}