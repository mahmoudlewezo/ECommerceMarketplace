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
        // =========================
        // Products Management
        // =========================

        // GET: /Admin/Products
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .OrderByDescending(p => p.Id)
                .Select(p => new AdminProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name,
                    SellerName = p.Seller.FullName
                })
                .ToListAsync();

            return View(products);
        }

        // POST: /Admin/RemoveProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var imageUrl = product.ImageUrl;

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            // Delete product image from wwwroot
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var fileName = Path.GetFileName(imageUrl);

                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "products",
                    fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            TempData["Success"] = "Product removed successfully.";

            return RedirectToAction(nameof(Products));
        }
        // =========================
        // Category Management
        // =========================

        // GET: /Admin/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Admin/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new CategoryViewModel());
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var name = model.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");

                return View(model);
            }

            var category = new Category
            {
                Name = name,
                Description = model.Description?.Trim()
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Categories));
        }

        // GET: /Admin/EditCategory/5
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (category == null)
                return NotFound();

            var name = model.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c =>
                    c.Id != model.Id &&
                    c.Name.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");

                return View(model);
            }

            category.Name = name;
            category.Description = model.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] =
                    "This category cannot be deleted because it contains products.";

                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Categories));
        }
    }
}