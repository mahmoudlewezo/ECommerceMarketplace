using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "The cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.Product.AvailableQuantity)
                {
                    TempData["Error"] = $"The available quantity is not sufficient for the product: {item.Product.Name}";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var order = new Order
            {
                CustomerId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalPrice = cart.CartItems.Sum(i => i.Quantity * i.Product.Price),
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                item.Product.AvailableQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);


            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = "The order has been created successfully!";
            return RedirectToAction("Index", "Cart");
        }
    }
}
