namespace ECommerceMarketplace.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string SellerId { get; set; } = string.Empty;

        public Category Category { get; set; } = null!;

        public ApplicationUser Seller { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
