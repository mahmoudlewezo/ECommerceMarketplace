using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

        // Seed test data for development: categories, test users, and products.
        // This method is idempotent and safe to call on each startup.
        public static async Task SeedTestDataAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Ensure roles exist (should already be created by SeedAsync, but double-check)
            string[] roles = { "Admin", "Seller", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 1) Seed Categories
            var categoryNames = new[]
            {
                "Fashion",
                "Beauty",
                "Electronics",
                "Home & Living",
                "Accessories",
                "Books & Stationery"
            };

            foreach (var name in categoryNames)
            {
                if (!context.Categories.Any(c => c.Name == name))
                {
                    context.Categories.Add(new Models.Category
                    {
                        Name = name,
                        Description = name + " products for Asrar marketplace"
                    });
                }
            }

            await context.SaveChangesAsync();

            // 2) Seed test users (2 sellers, 2 customers)
            // Development-only credentials (do NOT use in production)
            const string seller1Email = "seller1@asrar.dev";
            const string seller2Email = "seller2@asrar.dev";
            const string customer1Email = "customer1@asrar.dev";
            const string customer2Email = "customer2@asrar.dev";
            const string sellerPassword = "Seller@123";
            const string customerPassword = "Customer@123";

            // Helper to create user if missing
            async Task<ApplicationUser> EnsureUserAsync(string email, string fullName, string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        // If creation fails, return the user object as-is (it will be null in most cases)
                        return user;
                    }
                }

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                return user;
            }

            var seller1 = await EnsureUserAsync(seller1Email, "Seller One", sellerPassword, "Seller");
            var seller2 = await EnsureUserAsync(seller2Email, "Seller Two", sellerPassword, "Seller");
            var customer1 = await EnsureUserAsync(customer1Email, "Customer One", customerPassword, "Customer");
            var customer2 = await EnsureUserAsync(customer2Email, "Customer Two", customerPassword, "Customer");

            // 3) Seed Products: 3 products per category (18 total), distributed across seller1 and seller2
            // Prepare category lookup
            var categories = context.Categories.ToDictionary(c => c.Name, c => c.Id);

            // Basic product generator per category
            var productsToEnsure = new List<Product>();

            void AddProductSample(string categoryName, string name, string desc, decimal price, int qty, ApplicationUser seller)
            {
                if (!categories.ContainsKey(categoryName)) return;

                productsToEnsure.Add(new Product
                {
                    Name = name,
                    Description = desc,
                    Price = price,
                    AvailableQuantity = qty,
                    CategoryId = categories[categoryName],
                    SellerId = seller.Id,
                    ImageUrl = null // images to be added later; filenames will be listed below
                });
            }

            // Fashion
            AddProductSample("Fashion", "Classic White T-Shirt", "Comfortable cotton t-shirt for everyday wear.", 15.99m, 100, seller1);
            AddProductSample("Fashion", "Blue Denim Jeans", "Slim-fit denim jeans with stretch for comfort.", 49.50m, 60, seller2);
            AddProductSample("Fashion", "Women Summer Dress", "Lightweight floral summer dress perfect for warm days.", 39.00m, 40, seller1);

            // Beauty
            AddProductSample("Beauty", "Hydrating Face Cream", "24-hour hydration cream suitable for all skin types.", 18.75m, 200, seller2);
            AddProductSample("Beauty", "Natural Lipstick", "Long-lasting lipstick with natural pigments.", 12.00m, 150, seller1);
            AddProductSample("Beauty", "Aromatic Body Wash", "Gentle body wash with natural extracts.", 9.99m, 120, seller2);

            // Electronics
            AddProductSample("Electronics", "Wireless Earbuds", "Bluetooth 5.2 earbuds with noise reduction.", 59.99m, 80, seller1);
            AddProductSample("Electronics", "Portable Charger 10000mAh", "Compact power bank for phones and tablets.", 24.99m, 140, seller2);
            AddProductSample("Electronics", "Smart LED Bulb", "Wi-Fi enabled RGB LED bulb compatible with voice assistants.", 14.49m, 90, seller1);

            // Home & Living
            AddProductSample("Home & Living", "Cotton Bed Sheet Set", "Soft 100% cotton bed sheets 4-piece set.", 69.99m, 30, seller2);
            AddProductSample("Home & Living", "Ceramic Vase", "Handmade ceramic vase for home decor.", 29.50m, 50, seller1);
            AddProductSample("Home & Living", "Aroma Diffuser", "Ultrasonic diffuser for essential oils.", 22.00m, 75, seller2);

            // Accessories
            AddProductSample("Accessories", "Leather Wallet", "Genuine leather bi-fold wallet with card slots.", 25.00m, 110, seller1);
            AddProductSample("Accessories", "Sunglasses UV400", "Stylish sunglasses with UV400 protection.", 19.99m, 95, seller2);
            AddProductSample("Accessories", "Canvas Tote Bag", "Durable tote bag for shopping and daily use.", 14.00m, 180, seller1);

            // Books & Stationery
            AddProductSample("Books & Stationery", "Notebook A5 - Lined", "Hardcover lined notebook for notes and journaling.", 7.50m, 300, seller2);
            AddProductSample("Books & Stationery", "Beginner's Guide to Photography", "A friendly introduction to DSLR photography.", 21.99m, 40, seller1);
            AddProductSample("Books & Stationery", "Gel Pen Set", "Set of 12 smooth-writing gel pens.", 6.25m, 250, seller2);

            // Idempotent insert: create products that do not already exist (matching by Name and SellerId)
            foreach (var p in productsToEnsure)
            {
                var exists = context.Products.Any(x => x.Name == p.Name && x.SellerId == p.SellerId && x.CategoryId == p.CategoryId);
                if (!exists)
                {
                    context.Products.Add(p);
                }
            }

            await context.SaveChangesAsync();

            // Note: Images are currently not added. The following filenames would be needed under wwwroot/uploads/products/ (JFIF files were provided):
            // fashion-classic-white-tshirt.jfif
            // fashion-blue-denim-jeans.jfif
            // fashion-women-summer-dress.jfif
            // beauty-hydrating-face-cream.jfif
            // beauty-natural-lipstick.jfif
            // beauty-aromatic-body-wash.jfif
            // electronics-wireless-earbuds.jfif
            // electronics-portable-charger-10000mah.jfif
            // electronics-smart-led-bulb.jfif
            // home-cotton-bed-sheet-set.jfif
            // home-ceramic-vase.jfif
            // home-aroma-diffuser.jfif
            // accessories-leather-wallet.jfif
            // accessories-sunglasses-uv400.jfif
            // accessories-canvas-tote-bag.jfif
            // books-notebook-a5-lined.jfif
            // books-beginners-guide-photography.jfif
            // books-gel-pen-set.jfif

            // Update ImageUrl for seeded products if null or empty and if the corresponding image file exists.
            var imageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Classic White T-Shirt", "fashion-classic-white-tshirt.jfif" },
                { "Blue Denim Jeans", "fashion-blue-denim-jeans.jfif" },
                { "Women Summer Dress", "fashion-women-summer-dress.jfif" },
                { "Hydrating Face Cream", "beauty-hydrating-face-cream.jfif" },
                { "Natural Lipstick", "beauty-natural-lipstick.jfif" },
                { "Aromatic Body Wash", "beauty-aromatic-body-wash.jfif" },
                { "Wireless Earbuds", "electronics-wireless-earbuds.jfif" },
                { "Portable Charger 10000mAh", "electronics-portable-charger-10000mah.jfif" },
                { "Smart LED Bulb", "electronics-smart-led-bulb.jfif" },
                { "Cotton Bed Sheet Set", "home-cotton-bed-sheet-set.jfif" },
                { "Ceramic Vase", "home-ceramic-vase.jfif" },
                { "Aroma Diffuser", "home-aroma-diffuser.jfif" },
                { "Leather Wallet", "accessories-leather-wallet.jfif" },
                { "Sunglasses UV400", "accessories-sunglasses-uv400.jfif" },
                { "Canvas Tote Bag", "accessories-canvas-tote-bag.jfif" },
                { "Notebook A5 - Lined", "books-notebook-a5-lined.jfif" },
                { "Beginner's Guide to Photography", "books-beginners-guide-photography.jfif" },
                { "Gel Pen Set", "books-gel-pen-set.jfif" }
            };

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

            foreach (var kv in imageMap)
            {
                var product = await context.Products.FirstOrDefaultAsync(p => p.Name == kv.Key);
                if (product == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                    continue; // already set

                var filename = kv.Value;
                var filePath = Path.Combine(uploadsDir, filename);
                if (File.Exists(filePath))
                {
                    product.ImageUrl = "/uploads/products/" + filename;
                }
            }

            await context.SaveChangesAsync();

        }
    }
}
