using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Application.Interfaces.Services;
public  interface IProductService
{
    Task<PaginatedList<ProductDto>> GetAllProductsAsync(QueryParams  query);
    Task<Result<ProductDto>> GetProductByIdAsync(int id);
    Task<Result<ProductDto>> AddProductAsync(CreateProductDto createProductDto);
    Task<Result> UpdateProductAsync(UpdateProductDto updateProductDto);
    Task<Result> DeleteProductAsync(int id);
    Task<Result> UpdateStockAsync(int productId, int quantityChange);

}