using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Category.Admin;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Area("Admin")]
[Route("admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CreateCategoryDto> _createCategoryValidator;
    private readonly IValidator<UpdateCategoryDto> _updateCategoryValidator;

    public AdminCategoryController(ICategoryService categoryService, 
        IValidator<CreateCategoryDto> createCategoryValidator, 
        IValidator<UpdateCategoryDto> updateCategoryValidator)
    {
        _categoryService = categoryService;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
    }

    [HttpGet("all-categories")]
    public async Task<IActionResult> GetAllCategories([FromQuery] QueryParams query)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(query);
        return Ok(categories);
    }

    [HttpGet("{categoryId}")] 
    public async Task<IActionResult> GetCategoryById(int categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var result = await _categoryService.CreateCategoryAsync(createCategoryDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = result.Value!.CategoryId }, result.Value);
    }

    [HttpPut("{categoryId}")]
    public async Task<ActionResult> UpdateCategory([FromRoute] int categoryId, UpdateCategoryDto updateCategoryDto)
    {
        updateCategoryDto.CategoryId = categoryId;
        
        var result = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return Ok(result.Value);
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        var result = await _categoryService.DeleteCategoryAsync(categoryId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return NoContent();
    }
}
