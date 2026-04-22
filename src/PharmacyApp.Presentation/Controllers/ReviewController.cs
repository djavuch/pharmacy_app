using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Contracts.Review;

namespace PharmacyApp.Presentation.Controllers;

[ApiController]
[Route("products/{productId}")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewController(IReviewService reviewService)
    {
         _reviewService = reviewService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReview(int productId, int id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        if (result.Value!.ProductId != productId)
            return BadRequest("Review does not belong to this product.");
        
        return Ok(result.Value);
    }

    [HttpGet("all-reviews")]
    public async Task<IActionResult> GetAllReviews(int productId, [FromQuery] ReviewQueryParams queryParams)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId, queryParams);
        return Ok(reviews);
    }

    [HttpPost("add-review")]
    [Authorize] 
    public async Task<IActionResult> AddReview(int productId, CreateProductReviewDto reviewDto)
    {
        if (productId != reviewDto.ProductId)
        {
            return BadRequest("Product Id in URL does not match Product Id in body.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _reviewService.AddReviewAsync(reviewDto, userId);
        
        if (!result.IsSuccess)
            return StatusCode(result.ErrorType.ToStatusCode(), new { message = result.Message });
        
        return CreatedAtAction(nameof(GetReview), new { productId, id = result.Value!.Id }, result.Value);
    }
}
