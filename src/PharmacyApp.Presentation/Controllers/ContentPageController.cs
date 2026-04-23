using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[EnableCors("AllowFrontend")]
[Route("content-pages")]
public class ContentPageController : ControllerBase
{
    private readonly IContentPageService _contentPageService;

    public ContentPageController(IContentPageService contentPageService)
    {
        _contentPageService = contentPageService;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _contentPageService.GetPublishedBySlugAsync(slug);
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });

        return Ok(result.Value);
    }
}
