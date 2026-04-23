using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Common.Results;
using PharmacyApp.Application.Contracts.Product;
using PharmacyApp.Application.Interfaces.Abstractions;
using PharmacyApp.Application.Interfaces.Services;
using PharmacyApp.Infrastructure.Services.FileStorage;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[EnableCors("AllowFrontend")]
[Area("Admin")]
[Route("admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWishlistService _wishlistService;
    private readonly IImageStorageService _imageStorageService;

    public AdminProductController(IProductService productService, 
        IWishlistService wishlistService, IImageStorageService imageStorageService)
    {
        _productService = productService;
        _wishlistService = wishlistService;
        _imageStorageService = imageStorageService;
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
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
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
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
    {
        var result = await _productService.UpdateProductAsync(updateProductDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }

    [HttpPost("upload-image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<UploadImageStorageResponseDto>> UploadProductImage(IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });
        
        await using var stream = file.OpenReadStream();
        
        var imageUrl = await _imageStorageService.UploadImageAsync(stream, file.FileName, file.ContentType, cancellationToken);
        
        return Ok(new UploadImageStorageResponseDto(imageUrl));
    }
}
