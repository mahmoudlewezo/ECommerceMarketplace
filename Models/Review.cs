namespace ECommerceMarketplace.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; } = null!;

        public ApplicationUser Customer { get; set; } = null!;
    }
}