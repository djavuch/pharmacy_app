using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[Route("store/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(int pageIndex = 1, int pageSize = 10)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize);
        return Ok(categories);
    }
    
    [HttpGet("{id}")]
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
