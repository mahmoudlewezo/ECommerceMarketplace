using Microsoft.AspNetCore.Identity;

namespace ECommerceMarketplace.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<SellerRequest> SellerRequests { get; set; } = new List<SellerRequest>();

        public Cart? Cart { get; set; }

        public Wishlist? Wishlist { get; set; }
    }
}
