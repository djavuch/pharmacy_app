using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Admin.ProductCategory;
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
    public async Task<IActionResult> GetAllCategories(int pageIndex = 1, int pageSize = 10)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize);
        return Ok(categories);
    }

    [HttpGet("{categoryId}")] 
    public async Task<IActionResult> GetCategoryById(int categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var validationResult = await _createCategoryValidator.ValidateAsync(createCategoryDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var category = await _categoryService.CreateCategoryAsync(createCategoryDto);

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { categoryId = category.CategoryId },
            category);
    }

    [HttpPut("{categoryId}")]
    public async Task<ActionResult> UpdateCategory([FromRoute] int categoryId, UpdateCategoryDto updateCategoryDto)
    {
        updateCategoryDto.CategoryId = categoryId;

        var validationResult = await _updateCategoryValidator.ValidateAsync(updateCategoryDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var category = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
        return Ok(category);
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        await _categoryService.DeleteCategoryAsync(categoryId);
        return NoContent();
    }
}
