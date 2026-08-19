using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMarketplace.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class SellerRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SellerRequests
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.SellerRequests
                .Include(r => r.User)
                .Include(r => r.ReviewedBy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // POST: /SellerRequests/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.SellerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.Status != SellerRequestStatus.Pending)
            {
                TempData["Info"] = "This request has already been reviewed.";
                return RedirectToAction(nameof(Index));
            }

            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return Challenge();

            var user = request.User;

            if (!await _userManager.IsInRoleAsync(user, "Seller"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Seller");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View("Index", await GetRequests());
                }
            }

            // Remove Customer role after becoming Seller
            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, "Customer");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View("Index", await GetRequests());
                }
            }

            request.Status = SellerRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = admin.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{user.FullName}'s seller request has been approved.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /SellerRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.SellerRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.Status != SellerRequestStatus.Pending)
            {
                TempData["Info"] = "This request has already been reviewed.";
                return RedirectToAction(nameof(Index));
            }

            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return Challenge();

            request.Status = SellerRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = admin.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{request.User.FullName}'s seller request has been rejected.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SellerRequest>> GetRequests()
        {
            return await _context.SellerRequests
                .Include(r => r.User)
                .Include(r => r.ReviewedBy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
       
    }
}