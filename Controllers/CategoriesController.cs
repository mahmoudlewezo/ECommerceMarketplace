using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using ECommerceMarketplace.ViewModels.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMarketplace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryIndexViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Categories/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nameExists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");

                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim()
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: /Categories/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var model = new EditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (category == null)
                return NotFound();

            var nameExists = await _context.Categories
                .AnyAsync(c =>
                    c.Id != model.Id &&
                    c.Name.ToLower() == model.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");

                return View(model);
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] =
                    "You cannot delete a category that contains products.";

                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
