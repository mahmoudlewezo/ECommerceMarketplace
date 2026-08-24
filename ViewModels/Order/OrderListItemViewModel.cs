namespace ECommerceMarketplace.ViewModels.Order
{
    public class OrderListItemViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
    }
}
