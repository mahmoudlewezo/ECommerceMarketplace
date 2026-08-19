namespace ECommerceMarketplace.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public ApplicationUser Customer { get; set; } = null!;

        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
