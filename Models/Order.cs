namespace ECommerceMarketplace.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public ApplicationUser Customer { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}