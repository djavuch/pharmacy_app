using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Common;
using PharmacyApp.Application.DTOs.Product;
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
    public async Task<ActionResult<ApiResponse>> GetAllProducts(int pageIndex = 1, int pageSize = 10)
    {
        var products = await _productService.GetAllProductsAsync(pageIndex, pageSize);
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

    [HttpGet("product/{productId}/users")]
    public async Task<IActionResult> GetUsersWhoAddedProduct(int productId)
    {
        var users = await _wishlistService.GetUsersWhoAddedProductAsync(productId);
        return Ok(users);
    }
    
    [HttpPost("add")]
    public async Task<ActionResult<ProductDto>> AddProduct(CreateProductDto createProductDto)
    {
        var product = await _productService.AddProductAsync(createProductDto);
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
    {
        await _productService.UpdateProductAsync(updateProductDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
    
}
