using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Contracts.Content.Admin;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/content-pages")]
[Authorize(Roles = "Admin")]
public class AdminContentPageController : ControllerBase
{
    private readonly IContentPageService _contentPageService;

    public AdminContentPageController(IContentPageService contentPageService)
    {
        _contentPageService = contentPageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pages = await _contentPageService.GetAllAsync();
        return Ok(pages);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _contentPageService.GetBySlugForAdminAsync(slug);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return Ok(result.Value);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateContentPageDto dto)
    {
        var updatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _contentPageService.UpdateAsync(slug, dto, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return Ok(result.Value);
    }
}
