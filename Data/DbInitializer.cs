using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Identity;

namespace ECommerceMarketplace.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Create Roles
            string[] roles =
            {
                "Admin",
                "Seller",
                "Customer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // Create Admin
            const string adminEmail = "admin@ecommerce.com";
            const string adminPassword = "Admin@123";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Admin");
                }
            }
        }
    }
}
