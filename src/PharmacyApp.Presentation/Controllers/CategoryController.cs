using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("store/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] QueryParams query)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(query);
        return Ok(categories);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }
        return Ok(category);
    }
}
