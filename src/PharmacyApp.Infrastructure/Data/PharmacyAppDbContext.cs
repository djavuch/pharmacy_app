using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.Bonus;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Entities.PromoCode;
using System.Reflection;

namespace PharmacyApp.Infrastructure.Data;
public class PharmacyAppDbContext : IdentityDbContext<User>
{
    public PharmacyAppDbContext(DbContextOptions<PharmacyAppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ShoppingCart> ShoppingCart { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OrderAddress> OrderAddresses { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<ProductDiscount> ProductDiscounts { get; set; }
    public DbSet<CategoryDiscount> CategoryDiscounts { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<PromoCodeProduct> PromoCodeProducts { get; set; }
    public DbSet<PromoCodeCategory> PromoCodeCategories { get; set; }
    public DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }
    public DbSet<BonusAccount> BonusAccounts { get; set; }
    public DbSet<BonusTransaction> BonusTransactions { get; set; }
    public DbSet<BonusSettings> BonusSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}