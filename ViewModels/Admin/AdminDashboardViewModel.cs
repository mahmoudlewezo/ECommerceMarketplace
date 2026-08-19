namespace ECommerceMarketplace.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int Customers { get; set; }

        public int Sellers { get; set; }

        public int Admins { get; set; }

        public int SuspendedUsers { get; set; }

        public int PendingSellerRequests { get; set; }
    }
}
