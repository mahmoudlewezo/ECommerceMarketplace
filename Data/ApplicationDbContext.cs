using ECommerceMarketplace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMarketplace.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SellerRequest> SellerRequests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Product -> Category
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Seller (ApplicationUser)
            builder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart -> Customer
            builder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> CartItems
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Product
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wishlist -> Customer
            builder.Entity<Wishlist>()
                .HasOne(w => w.Customer)
                .WithOne(u => u.Wishlist)
                .HasForeignKey<Wishlist>(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist -> WishlistItems
            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            // WishlistItem -> Product
            builder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> Customer
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Product
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review -> Customer
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // SellerRequest -> User
            builder.Entity<SellerRequest>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SellerRequests)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}