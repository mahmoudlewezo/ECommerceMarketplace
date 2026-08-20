namespace ECommerceMarketplace.ViewModels.Admin
{
    public class AdminProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int AvailableQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;
    }
}
