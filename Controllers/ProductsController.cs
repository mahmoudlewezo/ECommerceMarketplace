using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using ECommerceMarketplace.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMarketplace.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Products/MyProducts
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.SellerId == user.Id)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Select(p => new ProductIndexViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return View(products);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();

            return View(new CreateProductViewModel());
        }

        // POST: /Products/Create
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                await LoadCategories();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                AvailableQuantity = model.AvailableQuantity,
                CategoryId = model.CategoryId,
                SellerId = user.Id
            };

            if (model.Image != null)
            {
                product.ImageUrl = await SaveImageAsync(model.Image);
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(MyProducts));
        }

        // GET: /Products/Edit/5
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var model = new EditProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                CategoryId = product.CategoryId,
                CurrentImageUrl = product.ImageUrl
            };

            await LoadCategories();

            return View(model);
        }

        // POST: /Products/Edit/5
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // IMPORTANT:
            // SellerId check prevents editing another seller's product.
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == model.Id &&
                    p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                model.CurrentImageUrl = product.ImageUrl;

                await LoadCategories();

                return View(model);
            }

            product.Name = model.Name.Trim();
            product.Description = model.Description.Trim();
            product.Price = model.Price;
            product.AvailableQuantity = model.AvailableQuantity;
            product.CategoryId = model.CategoryId;

            if (model.Image != null)
            {
                var oldImage = product.ImageUrl;

                product.ImageUrl = await SaveImageAsync(model.Image);

                DeleteImage(oldImage);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully.";

            return RedirectToAction(nameof(MyProducts));
        }

        // GET: /Products/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var model = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SellerName = product.Seller.FullName
            };

            return View(model);
        }

        // GET: /Products/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var model = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SellerName = user.FullName
            };

            return View(model);
        }

        // POST: /Products/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // IMPORTANT:
            // Seller can delete ONLY his own product.
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            var imageUrl = product.ImageUrl;

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            DeleteImage(imageUrl);

            TempData["Success"] = "Product deleted successfully.";

            return RedirectToAction(nameof(MyProducts));
        }
        // GET: /Products/Catalog
        [HttpGet]
        public async Task<IActionResult> Catalog(
            string? search,
            int? categoryId,
            string? sort)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            // Sort
            query = sort switch
            {
                "price_asc" =>
                    query.OrderBy(p => p.Price),

                "price_desc" =>
                    query.OrderByDescending(p => p.Price),

                "name_asc" =>
                    query.OrderBy(p => p.Name),

                "name_desc" =>
                    query.OrderByDescending(p => p.Name),

                _ =>
                    query.OrderByDescending(p => p.Id)
            };

            var products = await query
                .Select(p => new ProductIndexViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            await LoadCategories();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Sort = sort;

            return View(products);
        }

        // =========================
        // Helpers
        // =========================

        private async Task LoadCategories()
        {
            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(image.FileName);

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(uploadsFolder, fileName);

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await image.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var fileName = Path.GetFileName(imageUrl);

            var filePath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "products",
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
