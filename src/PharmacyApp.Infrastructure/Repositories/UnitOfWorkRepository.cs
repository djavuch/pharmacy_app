using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.RefreshTokens;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Data;
using System.Data;

namespace PharmacyApp.Infrastructure.Repositories;
public class UnitOfWorkRepository : IUnitOfWorkRepository
{
    private readonly PharmacyAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILoggerFactory _loggerFactory;
    private IUserRepository _userRepository;
    private IAuthRepository _authRepository;
    private IRefreshTokenRepository _refreshTokenRepository;
    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;
    private IOrderRepository? _orderRepository;
    private IShoppingCartRepository? _shoppingCartRepository;
    private IReviewRepository _reviewRepository;
    private IWishlistRepository? _wishlistRepository;
    private IDiscountRepository? _discountRepository;
    private IUserAddressRepository? _userAddressRepository;
    private IPromoCodeRepository? _promoCodeRepository;
    private IBonusRepository? _bonusRepository;
    private IContentPageRepository? _contentPageRepository;

    public UnitOfWorkRepository(
        PharmacyAppDbContext dbContext,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _loggerFactory = loggerFactory;
    }

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_dbContext, _userManager);

    public IUserAddressRepository UserAddresses =>
        _userAddressRepository ??= new UserAddressRepository(_dbContext);

    public IAuthRepository Auth =>
        _authRepository ??= new AuthRepository(_userManager, _signInManager, _loggerFactory.CreateLogger<AuthRepository>());

    public IRefreshTokenRepository RefreshTokens =>
        _refreshTokenRepository ??= new RefreshTokenRepository(_dbContext);

    public IProductRepository Products =>
        _productRepository ??= new ProductRepository(_dbContext);

    public ICategoryRepository Categories =>
        _categoryRepository ??= new CategoryRepository(_dbContext);

    public IOrderRepository Orders =>
        _orderRepository ??= new OrderRepository(_dbContext);

    public IShoppingCartRepository ShoppingCarts =>
        _shoppingCartRepository ??= new ShoppingCartRepository(_dbContext, _loggerFactory.CreateLogger <ShoppingCartRepository>());

    public IReviewRepository Reviews =>
        _reviewRepository ??= new ReviewRepository(_dbContext);

    public IWishlistRepository Wishlists =>
        _wishlistRepository ??= new WishlistRepository(_dbContext);

    public IDiscountRepository Discounts => 
        _discountRepository ??= new DiscountRepository(_dbContext);

    public IPromoCodeRepository PromoCodes =>
        _promoCodeRepository ??= new PromoCodeRepository(_dbContext);
    public IBonusRepository Bonuses =>
        _bonusRepository ??= new BonusRepository(_dbContext);

    public IContentPageRepository ContentPages =>
        _contentPageRepository ??= new ContentPageRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return await _dbContext.Database.BeginTransactionAsync(isolationLevel);
    }
}
