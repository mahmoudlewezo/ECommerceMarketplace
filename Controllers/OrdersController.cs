using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using ECommerceMarketplace.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If Admin, show all orders; otherwise show only customer's orders
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(o => o.CustomerId == userId);
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

        // GET: /Orders/Details/5
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

            if (!User.IsInRole("Admin") && order.CustomerId != userId)
                return Forbid();

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

        // POST: /Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (order.CustomerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed)
            {
                order.Status = OrderStatus.Cancelled;

                // Restock products
                foreach (var oi in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(oi.ProductId);
                    if (product != null)
                    {
                        product.AvailableQuantity += oi.Quantity;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Order cannot be cancelled at this stage.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
