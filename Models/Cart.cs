namespace ECommerceMarketplace.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public ApplicationUser Customer { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
