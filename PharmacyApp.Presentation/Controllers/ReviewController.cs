using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.DTOs.Review;
using PharmacyApp.Application.Interfaces.Services;
using System.Security.Claims;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(int productId, int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review is null) return NotFound();

        if (review.ProductId != productId)
            return BadRequest("Review does not belong to this product.");

        return Ok(review);
    }

    [HttpGet("all-reviews")]
    public async Task<IActionResult> GetAllReviews(
        int productId, 
        int pageIndex = 1, 
        int pageSize = 10)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId, pageIndex, pageSize);
        return Ok(reviews);
    }

    [HttpPost("add-review")]
    [Authorize] 
    public async Task<IActionResult> AddReview(int productId, CreateProductReviewDto reviewDto)
    {
        if (productId != reviewDto.ProductId)
        {
            return BadRequest("Product ID in URL does not match Product ID in body.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var createdReview = await _reviewService.AddReviewAsync(reviewDto, userId);

        return CreatedAtAction(nameof(GetReview), new { productId, id = createdReview.Id }, createdReview);
    }
}
