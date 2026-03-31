using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Entities.Bonus;
using PharmacyApp.Domain.Entities.Discount;
using PharmacyApp.Domain.Entities.PromoCode;
using System.Reflection;

namespace PharmacyApp.Infrastructure.Data;
public class PharmacyAppDbContext : IdentityDbContext<UserModel>
{
    public PharmacyAppDbContext(DbContextOptions<PharmacyAppDbContext> options) : base(options)
    {
    }

    public DbSet<ProductModel> Products { get; set; }
    public DbSet<ShoppingCartModel> ShoppingCart { get; set; }
    public DbSet<CartItemModel> CartItems { get; set; }
    public DbSet<WishlistModel> Wishlists { get; set; }
    public DbSet<CategoryModel> Categories { get; set; }
    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OrderItemModel> OrderItems { get; set; }
    public DbSet<ReviewModel> Reviews { get; set; }
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
    public DbSet<OrderAddressModel> OrderAddresses { get; set; }
    public DbSet<DiscountModel> Discounts { get; set; }
    public DbSet<ProductDiscountModel> ProductDiscounts { get; set; }
    public DbSet<CategoryDiscountModel> CategoryDiscounts { get; set; }
    public DbSet<UserAddressModel> UserAddresses { get; set; }
    public DbSet<PromoCodeModel> PromoCodes { get; set; }
    public DbSet<PromoCodeProductModel> PromoCodeProducts { get; set; }
    public DbSet<PromoCodeCategoryModel> PromoCodeCategories { get; set; }
    public DbSet<PromoCodeUsageModel> PromoCodeUsages { get; set; }
    public DbSet<BonusAccountModel> BonusAccounts { get; set; }
    public DbSet<BonusTransactionModel> BonusTransactions { get; set; }
    public DbSet<BonusSettingsModel> BonusSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}