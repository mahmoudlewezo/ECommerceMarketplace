using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Customer")]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(w => w.CustomerId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { CustomerId = userId, WishlistItems = new List<WishlistItem>() };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return View(wishlist);
        }

        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.CustomerId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { CustomerId = userId, WishlistItems = new List<WishlistItem>() };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            if (!wishlist.WishlistItems.Any(wi => wi.ProductId == productId))
            {
                wishlist.WishlistItems.Add(new WishlistItem { ProductId = productId });
                await _context.SaveChangesAsync();
                TempData["Success"] = "The product has been added to the wishlist successfully.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromWishlist(int wishlistItemId)
        {
            var item = await _context.WishlistItems.FindAsync(wishlistItemId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}