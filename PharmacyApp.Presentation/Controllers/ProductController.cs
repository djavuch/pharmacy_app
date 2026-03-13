using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Product;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("store/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetAllProducts(
        string? filterOn = null,
        string? filterQuery = null,
        bool? isAscending = null,
        string? sortBy = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var products = await _productService.GetAllProductsAsync(
            pageIndex,
            pageSize,
            filterOn,
            filterQuery,
            sortBy,
            isAscending ?? true);
        return new ApiResponse(true, null, products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpGet("category/{categoryName}")]
    public async Task<ActionResult<ApiResponse>> GetProductsByCategory(
        string categoryName,
        bool? isAscending,
        string? sortBy,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var products = await _productService.GetAllProductsAsync(
            pageIndex,
            pageSize,
            filterOn: "Category",
            filterQuery: categoryName,
            sortBy,
            isAscending ?? true);

        return new ApiResponse(true, null, products);
    }
}
