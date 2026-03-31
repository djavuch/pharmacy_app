using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Product;
using PharmacyApp.Application.Interfaces;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Application.Mappers;
using PharmacyApp.Domain.Entities;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

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

    public async Task<PaginatedList<ProductDto>> GetAllProductsAsync(
        int pageIndex = 1, 
        int pageSize = 10, 
        string? filterOn = null, 
        string? filterQuery = null, 
        string? sortBy = null, 
        bool isAscending = true)
    {
        var cacheKey = $"products_v{_cacheVersion}_{pageIndex}_{pageSize}_{filterOn}_{filterQuery}_{sortBy}_{isAscending}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                _logger.LogWarning("CACHE MISS - Loading products from database. Key: {CacheKey}", cacheKey);
                
                var productsQuery = _unitOfWork.Products.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    productsQuery = filterOn.ToLower() switch
                    {
                        "name" => productsQuery.Where(p => p.Name.ToLower().Contains(filterQuery.ToLower())),
                        "category" => productsQuery.Where(p => p.Category.CategoryName.ToLower().Contains(filterQuery.ToLower())),
                        "minprice" when decimal.TryParse(filterQuery, NumberStyles.Any, CultureInfo.InvariantCulture, out var minPrice) =>
                            productsQuery.Where(p => p.Price >= minPrice),
                        "maxprice" when decimal.TryParse(filterQuery, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxPrice) =>
                            productsQuery.Where(p => p.Price <= maxPrice),
                        "instock" => productsQuery.Where(p => p.StockQuantity > 0),
                        _ => productsQuery
                    };
                }

                productsQuery = (sortBy?.ToLower()) switch
                {
                    "name" => isAscending ? productsQuery.OrderBy(p => p.Name) : productsQuery.OrderByDescending(p => p.Name),
                    "price" => isAscending ? productsQuery.OrderBy(p => p.Price) : productsQuery.OrderByDescending(p => p.Price),
                    "stockquantity" => isAscending ? productsQuery.OrderBy(p => p.StockQuantity) : productsQuery.OrderByDescending(p => p.StockQuantity),
                    _ => productsQuery.OrderBy(p => p.Id)
                };

                var totalCount = await productsQuery.CountAsync();
                var skipResults = (pageIndex - 1) * pageSize;
                var products = await productsQuery.Skip(skipResults).Take(pageSize).ToListAsync();

                var productDtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    var discountedPrice = await _discountService.CalculateDiscountedPriceAsync(product.Id, product.CategoryId, product.Price);
                    productDtos.Add(product.ToProductDto(discountedPrice));
                }

                return new PaginatedList<ProductDto>
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Items = productDtos
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        return await _cache.GetOrCreateAsync(
            $"products_v{_cacheVersion}:id:{productId}",
            async cancel =>
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product is null)
                    return null;

                var discountedPrice = await _discountService.CalculateDiscountedPriceAsync(product.Id, product.CategoryId, product.Price);
                return product.ToProductDto(discountedPrice);
            },
            new HybridCacheEntryOptions()
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }
        );
    }

    public async Task<ProductDto> AddProductAsync(CreateProductDto createProductDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(createProductDto.CategoryId);

        if (category is null)
        {
            throw new ConflictException($"Category '{createProductDto.CategoryId}' does not exist.");
        }

        var product = new ProductModel
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description ?? string.Empty,
            Price = createProductDto.Price,
            StockQuantity = createProductDto.StockQuantity,
            ImageUrl = createProductDto.ImageUrl,
            Category = category
        };

        product.ValidateBusinessRules();

        var addedProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        InvalidateProductsCache();

        return product.ToProductDto();
    }

    public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(updateProductDto.ProductId);

        if (product is null)
        {
            throw new NotFoundException("Product not found");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(updateProductDto.CategoryId);

        if (category is null)
        {
            throw new ConflictException($"Category '{updateProductDto.CategoryId}' does not exist.");
        }

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.StockQuantity = updateProductDto.StockQuantity;
        product.ImageUrl = updateProductDto.ImageUrl;
        product.Category = category;

        product.ValidateBusinessRules();
        
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        InvalidateProductsCache();
    }

    public async Task DeleteProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);

        if (product is null) 
        {
            throw new NotFoundException("Product not found");
        }

        await _unitOfWork.Products.DeleteAsync(productId);
        await _unitOfWork.SaveChangesAsync();

        InvalidateProductsCache();
    }

    public async Task UpdateStockAsync(int productId, int quantityChange)
    {
        var semaphore = _productLocks.GetOrAdd(productId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            if (product is null)
            {
                throw new NotFoundException("Product not found");
            }

            if (product.StockQuantity + quantityChange < 0) 
            { 
                throw new ConflictException("Insufficient stock to decrease.");      
            }

            product.StockQuantity += quantityChange;
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            InvalidateProductsCache();
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
