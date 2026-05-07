using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Contracts.Discount;
using PharmacyApp.Application.Contracts.Promotion;
using PharmacyApp.Application.Contracts.ShoppingCart;
using PharmacyApp.Application.Interfaces.RefreshTokens;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Services;
using PharmacyApp.Domain.Common;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.UnitTests.Services;

public sealed class ShoppingCartServiceTests
{
    [Fact]
    public async Task AddToCartAsync_WhenIdentifiersAreMissing_ReturnsConflict()
    {
        var service = CreateService();

        var result = await service.AddToCartAsync(null, null, new AddToCartDto
        {
            ProductId = 10,
            Quantity = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task AddToCartAsync_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.AddToCartAsync(null, "guest-session", new AddToCartDto
        {
            ProductId = 10,
            Quantity = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task AddToCartAsync_WhenRequestedQuantityExceedsStock_ReturnsConflict()
    {
        var product = CreateProduct(stockQuantity: 1);
        var service = CreateService(products: new FakeProductRepository(product));

        var result = await service.AddToCartAsync(null, "guest-session", new AddToCartDto
        {
            ProductId = 10,
            Quantity = 2
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task AddToCartAsync_WhenItemAlreadyExists_IncrementsQuantity()
    {
        var product = CreateProduct(stockQuantity: 5);
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m)
        {
            Product = product
        };
        var cart = CreateCartWithId(1, null, "guest-session");
        cart.Items.Add(item);
        var carts = new FakeShoppingCartRepository(cart);
        var unitOfWork = new FakeUnitOfWorkRepository(new FakeProductRepository(product), carts);
        var service = CreateService(unitOfWork);

        var result = await service.AddToCartAsync(null, "guest-session", new AddToCartDto
        {
            ProductId = 10,
            Quantity = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(1, carts.UpdateItemCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal(50m, result.Value!.TotalPrice);
    }

    [Fact]
    public async Task AddToCartAsync_WhenItemDoesNotExist_AddsNewItem()
    {
        var product = CreateProduct(stockQuantity: 5);
        var cart = CreateCartWithId(1, null, "guest-session");
        var carts = new FakeShoppingCartRepository(cart);
        var unitOfWork = new FakeUnitOfWorkRepository(new FakeProductRepository(product), carts);
        var service = CreateService(unitOfWork);

        var result = await service.AddToCartAsync(null, "guest-session", new AddToCartDto
        {
            ProductId = 10,
            Quantity = 2
        });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(cart.Items);
        Assert.Equal(10, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(1, carts.AddItemCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal(50m, result.Value!.TotalPrice);
    }

    [Fact]
    public async Task AddToCartAsync_WhenTotalQuantityWouldExceedStock_ReturnsConflict()
    {
        var product = CreateProduct(stockQuantity: 2);
        var item = new CartItem(cartId: 1, productId: 10, quantity: 2, priceAtAdd: 25m);
        var cart = CreateCartWithId(1, null, "guest-session");
        cart.Items.Add(item);
        var unitOfWork = new FakeUnitOfWorkRepository(
            new FakeProductRepository(product),
            new FakeShoppingCartRepository(cart));
        var service = CreateService(unitOfWork);

        var result = await service.AddToCartAsync(null, "guest-session", new AddToCartDto
        {
            ProductId = 10,
            Quantity = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateCartItemAsync_WhenQuantityIsZero_RemovesItem()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 2, priceAtAdd: 25m);
        var cart = CreateCartWithId(1, null, "guest-session");
        cart.Items.Add(item);
        var carts = new FakeShoppingCartRepository(cart);
        var unitOfWork = new FakeUnitOfWorkRepository(new FakeProductRepository(), carts);
        var service = CreateService(unitOfWork);

        var result = await service.UpdateCartItemAsync(null, "guest-session", new UpdateCartDto
        {
            ProductId = 10,
            Quantity = 0
        });

        Assert.True(result.IsSuccess);
        Assert.Empty(cart.Items);
        Assert.Equal(1, carts.RemoveItemCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateCartItemAsync_WhenQuantityExceedsStock_ReturnsConflict()
    {
        var product = CreateProduct(stockQuantity: 2);
        var item = new CartItem(cartId: 1, productId: 10, quantity: 1, priceAtAdd: 25m);
        var cart = CreateCartWithId(1, null, "guest-session");
        cart.Items.Add(item);
        var service = CreateService(
            products: new FakeProductRepository(product),
            carts: new FakeShoppingCartRepository(cart));

        var result = await service.UpdateCartItemAsync(null, "guest-session", new UpdateCartDto
        {
            ProductId = 10,
            Quantity = 3
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public async Task RemoveCartItemAsync_WhenItemExists_RemovesItem()
    {
        var item = new CartItem(cartId: 1, productId: 10, quantity: 2, priceAtAdd: 25m);
        var cart = CreateCartWithId(1, null, "guest-session");
        cart.Items.Add(item);
        var carts = new FakeShoppingCartRepository(cart);
        var unitOfWork = new FakeUnitOfWorkRepository(new FakeProductRepository(), carts);
        var service = CreateService(unitOfWork);

        var result = await service.RemoveCartItemAsync(null, "guest-session", productId: 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(cart.Items);
        Assert.Equal(1, carts.RemoveItemCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ClearCartAsync_WhenCartExists_ClearsCartById()
    {
        var cart = CreateCartWithId(7, null, "guest-session");
        var carts = new FakeShoppingCartRepository(cart);
        var unitOfWork = new FakeUnitOfWorkRepository(new FakeProductRepository(), carts);
        var service = CreateService(unitOfWork);

        var result = await service.ClearCartAsync(null, "guest-session");

        Assert.True(result.IsSuccess);
        Assert.Equal(7, carts.ClearedCartId);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    private static ShoppingCartService CreateService(
        FakeProductRepository? products = null,
        FakeShoppingCartRepository? carts = null)
    {
        var unitOfWork = new FakeUnitOfWorkRepository(
            products ?? new FakeProductRepository(),
            carts ?? new FakeShoppingCartRepository());

        return CreateService(unitOfWork);
    }

    private static ShoppingCartService CreateService(FakeUnitOfWorkRepository unitOfWork)
    {
        return new ShoppingCartService(
            unitOfWork,
            new FakeDiscountService(),
            new NoOpLogger<ShoppingCartService>());
    }

    private static Product CreateProduct(int stockQuantity)
    {
        var category = new Category("Pain relief", "Pain relief products");
        return new Product("Aspirin", "Description", 25m, stockQuantity, "/image.png", category);
    }

    private static ShoppingCart CreateCartWithId(int id, string? userId, string? sessionId)
    {
        var cart = new ShoppingCart(userId!, sessionId);
        typeof(ShoppingCart)
            .GetProperty(nameof(ShoppingCart.Id))!
            .SetValue(cart, id);

        return cart;
    }

    private sealed class FakeUnitOfWorkRepository : IUnitOfWorkRepository
    {
        public FakeUnitOfWorkRepository(
            FakeProductRepository products,
            FakeShoppingCartRepository shoppingCarts)
        {
            Products = products;
            ShoppingCarts = shoppingCarts;
        }

        public IProductRepository Products { get; }
        public IShoppingCartRepository ShoppingCarts { get; }
        public int SaveChangesCalls { get; private set; }

        public IOrderRepository Orders => throw new NotSupportedException();
        public IReviewRepository Reviews => throw new NotSupportedException();
        public ICategoryRepository Categories => throw new NotSupportedException();
        public IUserRepository Users => throw new NotSupportedException();
        public IAuthRepository Auth => throw new NotSupportedException();
        public IRefreshTokenRepository RefreshTokens => throw new NotSupportedException();
        public IWishlistRepository Wishlists => throw new NotSupportedException();
        public IDiscountRepository Discounts => throw new NotSupportedException();
        public IUserAddressRepository UserAddresses => throw new NotSupportedException();
        public IPromoCodeRepository PromoCodes => throw new NotSupportedException();
        public IBonusRepository Bonuses => throw new NotSupportedException();
        public IContentPageRepository ContentPages => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Product? _product;

        public FakeProductRepository(Product? product = null)
        {
            _product = product;
        }

        public IQueryable<Product> GetAllAsync() => throw new NotSupportedException();
        public Task<Product?> GetByIdAsync(int productId) => Task.FromResult(_product);
        public Task<Product?> GetByIdWithCategoryAsync(int productId) => throw new NotSupportedException();
        public Task<Product> AddAsync(Product product) => throw new NotSupportedException();
        public Task UpdateAsync(Product product) => throw new NotSupportedException();
        public Task DeleteAsync(int productId) => throw new NotSupportedException();
        public Task<List<Product>> GetByIdsAsync(List<int> productIds) => throw new NotSupportedException();
        public Task<int> TryAdjustStockAsync(int productId, int quantityChange) => throw new NotSupportedException();
        public Task UpdateWishlistCountAsync(int productId, int delta) => throw new NotSupportedException();
    }

    private sealed class FakeShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ShoppingCart? _cart;

        public FakeShoppingCartRepository(ShoppingCart? cart = null)
        {
            _cart = cart;
        }

        public int AddItemCalls { get; private set; }
        public int UpdateItemCalls { get; private set; }
        public int RemoveItemCalls { get; private set; }
        public int? ClearedCartId { get; private set; }

        public Task<ShoppingCart?> GetByUserIdAsync(string userId) => Task.FromResult(_cart);
        public Task<ShoppingCart?> GetBySessionIdAsync(string sessionId) => Task.FromResult(_cart);
        public Task<ShoppingCart?> GetByUserOrSessionAsync(string? userId, string? sessionId) => Task.FromResult(_cart);
        public Task<CartItem?> GetItemAsync(int cartId, int productId) => throw new NotSupportedException();
        public Task<ShoppingCart> AddAsync(ShoppingCart cart) => throw new NotSupportedException();

        public Task UpdateAsync(ShoppingCart cart)
        {
            return Task.CompletedTask;
        }

        public Task AddItemAsync(CartItem cartItem)
        {
            AddItemCalls++;
            return Task.CompletedTask;
        }

        public Task UpdateItemAsync(CartItem cartItem)
        {
            UpdateItemCalls++;
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(CartItem cartItem)
        {
            RemoveItemCalls++;
            return Task.CompletedTask;
        }

        public Task ClearAsync(int cartId)
        {
            ClearedCartId = cartId;
            return Task.CompletedTask;
        }

        public Task MigrateCartAsync(string sessionId, string userId) => throw new NotSupportedException();
    }

    private sealed class FakeDiscountService : IDiscountService
    {
        public Task<Result<DiscountDto>> CreateDiscountAsync(CreateDiscountDto dto) => throw new NotSupportedException();
        public Task<DiscountDto?> GetDiscountByIdAsync(Guid discountId) => throw new NotSupportedException();
        public Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync() => throw new NotSupportedException();
        public Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync() => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PromotionListItemDto>> GetActivePromotionsAsync() => throw new NotSupportedException();
        public Task<Result<PromotionDetailsDto>> GetActivePromotionBySlugAsync(string slug) => throw new NotSupportedException();
        public Task<Result> UpdateDiscountAsync(Guid discountId, UpdateDiscountDto dto) => throw new NotSupportedException();
        public Task<Result> DeleteDiscountAsync(Guid discountId) => throw new NotSupportedException();
        public Task<decimal> CalculateDiscountedPriceAsync(int productId, int categoryId, decimal originalPrice) => Task.FromResult(originalPrice);

        public Task<Dictionary<int, decimal>> CalculateDiscountedPricesAsync(
            IReadOnlyCollection<ProductPriceContext> products)
        {
            return Task.FromResult(products.ToDictionary(product => product.ProductId, product => product.OriginalPrice));
        }
    }

    private sealed class NoOpLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
