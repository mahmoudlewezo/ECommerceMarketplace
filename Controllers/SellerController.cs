using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ECommerceMarketplace.ViewModels.Seller;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMarketplace.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Seller/BecomeSeller
        [HttpGet]
        public async Task<IActionResult> BecomeSeller()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                TempData["Info"] = "You are already a seller.";
                return RedirectToAction("Index", "Home");
            }

            var existingRequest = await _context.SellerRequests
                .FirstOrDefaultAsync(r =>
                    r.UserId == user.Id &&
                    r.Status == SellerRequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["Info"] =
                    "You already have a pending seller request.";

                return RedirectToAction("Index", "Home");
            }

            return View(new BecomeSellerViewModel());
        }


        // POST: /Seller/BecomeSeller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeSeller(
     BecomeSellerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                TempData["Info"] = "You are already a seller.";
                return RedirectToAction("Index", "Home");
            }

            var existingRequest = await _context.SellerRequests
                .FirstOrDefaultAsync(r =>
                    r.UserId == user.Id &&
                    r.Status == SellerRequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["Info"] =
                    "You already have a pending seller request.";

                return RedirectToAction("Index", "Home");
            }

            var request = new SellerRequest
            {
                UserId = user.Id,
                Reason = model.Reason.Trim(),
                Status = SellerRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.SellerRequests.Add(request);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your seller request has been submitted successfully.";

            return RedirectToAction("Index", "Home");
        }
    }
}