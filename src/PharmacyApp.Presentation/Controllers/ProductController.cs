using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.Product;
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
    public async Task<ActionResult<ApiResponse>> GetAllProducts([FromQuery] QueryParams query)
    {
        var products = await _productService.GetAllProductsAsync(query);
        return new ApiResponse(true, null, products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return Ok(result.Value);;
    }

    [HttpGet("category/{categoryName}")]
    public async Task<ActionResult<ApiResponse>> GetProductsByCategory(string categoryName, QueryParams query)
    {
        var categoryQuery = query with
        {
            FilterOn = "Category",
            FilterQuery = categoryName
        };
        
        var products = await _productService.GetAllProductsAsync(categoryQuery);

        return new ApiResponse(true, null, products);
    }
}
