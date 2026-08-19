using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceMarketplace.ViewModels.Admin;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var totalUsers = users.Count;

            var customers = 0;
            var sellers = 0;
            var admins = 0;
            var suspendedUsers = 0;

            foreach (var user in users)
            {
                if (!user.IsActive)
                {
                    suspendedUsers++;
                }

                if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    customers++;
                }

                if (await _userManager.IsInRoleAsync(user, "Seller"))
                {
                    sellers++;
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    admins++;
                }
            }

            var pendingSellerRequests =
                await _context.SellerRequests
                    .CountAsync(r =>
                        r.Status == SellerRequestStatus.Pending);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                Customers = customers,
                Sellers = sellers,
                Admins = admins,
                SuspendedUsers = suspendedUsers,
                PendingSellerRequests = pendingSellerRequests
            };

            return View(model);
        }
    }
}