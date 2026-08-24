using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using ECommerceMarketplace.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Seller,Admin")]
    public class SellerOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SellerOrders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Admin can see all orders; sellers see orders that include their products
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == userId));
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListItemViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalPrice = o.TotalPrice,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return View(orders);
        }

        // GET: /SellerOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Seller)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!User.IsInRole("Admin") && !order.OrderItems.All(oi => oi.Product.SellerId == userId) && !order.OrderItems.Any(oi => oi.Product.SellerId == userId))
            {
                // Seller can view order if at least one item belongs to them
                return Forbid();
            }

            var vm = new OrderDetailsViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalPrice = order.TotalPrice,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SellerId = oi.Product.SellerId,
                    SellerName = oi.Product.Seller.FullName
                }).ToList()
            };

            return View(vm);
        }

        // POST: /SellerOrders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (User.IsInRole("Admin"))
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Order status updated.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Seller: allow update only if ALL order items belong to this seller
            var allBelongToSeller = order.OrderItems.All(oi => oi.Product.SellerId == userId);

            if (!allBelongToSeller)
            {
                TempData["Error"] = "You cannot update status for orders containing other sellers' products.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order status updated.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
