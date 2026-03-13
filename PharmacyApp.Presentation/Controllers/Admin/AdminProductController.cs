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
    private readonly IValidator<AddProductDto> _addProductValidator;
    private readonly IValidator<UpdateProductDto> _updateProductValidator;

    public AdminProductController(IProductService productService, 
        IValidator<AddProductDto> addProductValidator, 
        IValidator<UpdateProductDto> updateProductValidator)
    {
        _productService = productService;
        _addProductValidator = addProductValidator;
        _updateProductValidator = updateProductValidator;
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

    [HttpPost("add")]
    public async Task<ActionResult<ProductDto>> AddProduct(AddProductDto addProductDto)
    {
        var validationResult = await _addProductValidator.ValidateAsync(addProductDto);

        if (!validationResult.IsValid) {
            return BadRequest(validationResult.Errors);
        }

        var product = await _productService.AddProductAsync(addProductDto);
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
    {
        var validationResult = await _updateProductValidator.ValidateAsync(updateProductDto);

        if (!validationResult.IsValid) {
            return BadRequest(validationResult.Errors);
        }

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
