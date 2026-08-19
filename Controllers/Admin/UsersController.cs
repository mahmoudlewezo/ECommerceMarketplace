using ECommerceMarketplace.Models;
using ECommerceMarketplace.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMarketplace.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Users
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            var model = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive
                });
            }

            return View(model);
        }

        // POST: /Users/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] =
                    "You cannot suspend an Admin.";

                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                $"{user.FullName} has been suspended.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.IsActive = true;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                $"{user.FullName} has been activated.";

            return RedirectToAction(nameof(Index));
        }
    }
}
