using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Application.Interfaces.Repositories;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Services;
public class ProductService : IProductService
{
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly HybridCache _cache;
    private readonly IDiscountService _discountService;
    private readonly ILogger<ProductService> _logger;   
    private static int _cacheVersion = 0;

    public ProductService(
        IUnitOfWorkRepository unitOfWork, 
        HybridCache cache, 
        IDiscountService discountService,
        ILogger<ProductService> logger)   
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _discountService = discountService;
        _logger = logger;
    }

    public async Task<PaginatedList<ProductDto>> GetAllProductsAsync(QueryParams query)
    {
        var cacheKey = CacheKeys.Products.All(_cacheVersion, query);

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                _logger.LogWarning("CACHE MISS - Loading products from database. Key: {CacheKey}", cacheKey);
                
                var productsQuery = _unitOfWork.Products.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(query.FilterOn) && !string.IsNullOrWhiteSpace(query.FilterQuery))
                {
                    productsQuery = query.FilterOn!.ToLower() switch
                    {
                        "name" => productsQuery.Where(p => p.Name.ToLower().Contains(query.FilterQuery.ToLower())),
                        "category" => productsQuery.Where(p => p.Category.CategoryName.ToLower().Contains(query.FilterQuery.ToLower())),
                        "minprice" when decimal.TryParse(query.FilterQuery, NumberStyles.Any, CultureInfo.InvariantCulture, out var minPrice) =>
                            productsQuery.Where(p => p.Price >= minPrice),
                        "maxprice" when decimal.TryParse(query.FilterQuery, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxPrice) =>
                            productsQuery.Where(p => p.Price <= maxPrice),
                        "instock" => productsQuery.Where(p => p.StockQuantity > 0),
                        _ => productsQuery
                    };
                }

                if (!string.IsNullOrWhiteSpace(query.CategoryName))
                {
                    var normalizedCategoryName = query.CategoryName.Trim().ToLower();
                    productsQuery = productsQuery.Where(p => p.Category.CategoryName.ToLower() == normalizedCategoryName);
                }

                if (query.SaleOnly)
                {
                    var now = DateTime.UtcNow;
                    productsQuery = productsQuery.Where(p =>
                        p.ProductDiscounts.Any(d => 
                            d.Discount.IsActive && 
                            d.Discount.StartDate <= now && 
                            d.Discount.EndDate >= now &&
                            d.Discount.Value > 0)
                        || p.Category.CategoryDiscounts.Any(cd => 
                            cd.Discount.IsActive && 
                            cd.Discount.StartDate <= now && 
                            cd.Discount.EndDate >= now &&
                            cd.Discount.Value > 0));
                }
    
                productsQuery = (query.SortBy?.ToLower()) switch
                {
                    "name" => query.IsAscending ? productsQuery.OrderBy(p => p.Name) : productsQuery.OrderByDescending(p => p.Name),
                    "price" => query.IsAscending ? productsQuery.OrderBy(p => p.Price) : productsQuery.OrderByDescending(p => p.Price),
                    "stockquantity" => query.IsAscending ? productsQuery.OrderBy(p => p.StockQuantity) : productsQuery.OrderByDescending(p => p.StockQuantity),
                    _ => productsQuery.OrderBy(p => p.Id)
                };

                var totalCount = await productsQuery.CountAsync();
                var products = await productsQuery
                    .Skip((query.PageIndex - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var productIds = products.Select(p => p.Id).ToList();
                var reviewStatsByProductId = await _unitOfWork.Reviews.GetApprovedStatsByIdsAsync(productIds);
                var priceMap = await _discountService.CalculateDiscountedPricesAsync(
                    products.Select(p => new ProductPriceContext(p.Id, p.CategoryId, p.Price)).ToList());

                var productDtos = new List<ProductDto>();
                
                foreach (var product in products)
                {
                    reviewStatsByProductId.TryGetValue(product.Id, out var reviewStats);
                    var discountedPrice = priceMap[product.Id];

                    productDtos.Add(product.ToProductDto(
                        discountedPrice,
                        reviewStats.Count,
                        reviewStats.AverageRating));
                }

                return PaginatedList<ProductDto>.Create(productDtos, totalCount, query);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<Result<ProductDto>> GetProductByIdAsync(int productId)
    {
        var productDto = await _cache.GetOrCreateAsync(
            CacheKeys.Products.ById(_cacheVersion, productId),
            async _ =>
            {
                var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(productId);
                if (product is null)
                    return null; 

                var reviewStats = await _unitOfWork.Reviews.GetApprovedStatsAsync(productId);
                var discountedPrice = await _discountService
                    .CalculateDiscountedPriceAsync(product.Id, product.CategoryId, product.Price);
                return product.ToProductDto(discountedPrice, reviewStats.Count, reviewStats.AverageRating);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );

        if (productDto is null)
            return Result<ProductDto>.NotFound($"Product with id '{productId}' was not found.");

        return Result<ProductDto>.Success(productDto);
    }

    public async Task<Result<ProductDto>> AddProductAsync(CreateProductDto createProductDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(createProductDto.CategoryId);

        if (category is null)
            return Result<ProductDto>.NotFound($"Category '{createProductDto.CategoryId}' does not exist.");

        var product = new Product(createProductDto.Name, createProductDto.Description ?? string.Empty,
            createProductDto.Price, createProductDto.StockQuantity, createProductDto.ImageUrl, category);
        
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        product.AssignProductCode(FormatProductCode(product.Id));
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateProductsCache();

        return Result<ProductDto>.Success(product.ToProductDto());
    }

    public async Task<Result> UpdateProductAsync(UpdateProductDto updateProductDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(updateProductDto.ProductId);

        if (product is null) 
            return Result.NotFound("Product not found");
        
        var category = await _unitOfWork.Categories.GetByIdAsync(updateProductDto.CategoryId);

        if (category is null)
            return Result.NotFound($"Category '{updateProductDto.CategoryId}' does not exist.");
        

        product.Update(updateProductDto.Name, updateProductDto.Description, updateProductDto.Price,
            updateProductDto.StockQuantity, updateProductDto.ImageUrl, category);
        
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateProductsCache();
        return Result.Success();
    }

    public async Task<Result> DeleteProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);

        if (product is null) 
            return Result.NotFound("Product not found");
        

        await _unitOfWork.Products.DeleteAsync(productId);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateProductsCache();
        return Result.Success();
    }

    public async Task<Result> UpdateStockAsync(int productId, int quantityChange)
    {
        if (quantityChange == 0)
        {
            return Result.Success();
        }

        var affectedRows = await _unitOfWork.Products.TryAdjustStockAsync(productId, quantityChange);
        if (affectedRows > 0)
        {
            InvalidateProductsCache();
            return Result.Success();
        }

        var exists = await _unitOfWork.Products.GetByIdAsync(productId) is not null;
        if (!exists)
            return Result.NotFound("Product not found");

        if (quantityChange < 0)
            return Result.Conflict("Insufficient stock to decrease.");

        return Result.Conflict("Unable to update stock.");
    }
    
    private static void InvalidateProductsCache()
    {
        Interlocked.Increment(ref _cacheVersion);
    }

    private static string FormatProductCode(int productId) => $"PRD-{productId:D6}";
}
