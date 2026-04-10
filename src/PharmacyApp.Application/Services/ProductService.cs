using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _productLocks = new();
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

                var productDtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    var discountedPrice = await _discountService.CalculateDiscountedPriceAsync(product.Id, product.CategoryId, product.Price);
                    productDtos.Add(product.ToProductDto(discountedPrice));
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
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product is null)
                    return null; 

                var discountedPrice = await _discountService
                    .CalculateDiscountedPriceAsync(product.Id, product.CategoryId, product.Price);
                return product.ToProductDto(discountedPrice);
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
        var semaphore = _productLocks.GetOrAdd(productId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            if (product is null)
                return Result.NotFound("Product not found");

            if (product.StockQuantity + quantityChange < 0) 
                return Result.Conflict("Insufficient stock to decrease.");      

            product.UpdateStockQuantity(quantityChange);
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            InvalidateProductsCache();
            return Result.Success();
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    private static void InvalidateProductsCache()
    {
        Interlocked.Increment(ref _cacheVersion);
    }
}
