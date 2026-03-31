using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Product;

namespace PharmacyApp.Application.Interfaces.Services;
public  interface IProductService
{
    Task<PaginatedList<ProductDto>> GetAllProductsAsync(int pageIndex = 1, int pageSize = 10, string? filterOn = null, 
        string filterQuery = null, string sortBy = null, bool isAscending = true);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> AddProductAsync(CreateProductDto createProductDto);
    Task UpdateProductAsync(UpdateProductDto updateProductDto);
    Task DeleteProductAsync(int id);
    Task UpdateStockAsync(int productId, int quantityChange);

}