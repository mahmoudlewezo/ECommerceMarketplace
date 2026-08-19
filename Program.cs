using ECommerceMarketplace.Data;
using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

namespace ECommerceMarketplace
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnValidatePrincipal = async context =>
                {
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();

                    var user = await userManager.GetUserAsync(context.Principal);

                    if (user == null || !user.IsActive)
                    {
                        context.RejectPrincipal();
                    }
                };
            });



            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // =========================
            // Seed Roles & Admin
            // =========================

            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider
                        .GetRequiredService<RoleManager<IdentityRole>>();

                var userManager =
                    scope.ServiceProvider
                        .GetRequiredService<UserManager<ApplicationUser>>();

                await DbInitializer.SeedAsync(
                    roleManager,
                    userManager);
            }

            // =========================
            // Middleware
            // =========================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}