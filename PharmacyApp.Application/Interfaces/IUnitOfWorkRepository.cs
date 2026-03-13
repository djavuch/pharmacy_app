using Microsoft.EntityFrameworkCore.Storage;
using PharmacyApp.Application.Interfaces.RefreshTokens;
using PharmacyApp.Application.Interfaces.Repositories;
using System.Data;

namespace PharmacyApp.Application.Interfaces;

public interface IUnitOfWorkRepository
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IShoppingCartRepository ShoppingCarts { get; }
    IReviewRepository Reviews { get; }
    ICategoryRepository Categories { get; }
    IUserRepository Users { get; }
    IAuthRepository Auth { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IWishlistRepository Wishlists { get; }
    IDiscountRepository Discounts { get; }
    IUserAddressRepository UserAddresses { get; }
    IPromoCodeRepository PromoCodes { get; }
    IBonusRepository Bonuses { get;  }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
