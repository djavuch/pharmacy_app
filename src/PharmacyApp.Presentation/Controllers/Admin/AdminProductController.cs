using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Area("Admin")]
[Route("admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWishlistService _wishlistService;

    public AdminProductController(IProductService productService, 
        IWishlistService wishlistService)
    {
        _productService = productService;
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetAllProducts([FromQuery] QueryParams query)
    {
        var products = await _productService.GetAllProductsAsync(query);
        return new ApiResponse(true, null, products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpGet("product/{productId}/users")]
    public async Task<IActionResult> GetUsersWhoAddedProduct(int productId)
    {
        var users = await _wishlistService.GetUsersWhoAddedProductAsync(productId);
        return Ok(users);
    }
    
    [HttpPost]
    public async Task<ActionResult<ProductDto>> AddProduct(CreateProductDto createProductDto)
    {
        var result = await _productService.AddProductAsync(createProductDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
    {
        var result = await _productService.UpdateProductAsync(updateProductDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode, new { message = result.Message });
        
        return NoContent();
    }
}
