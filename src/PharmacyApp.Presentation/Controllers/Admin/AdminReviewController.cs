using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Application.Common.Pagination;
using PharmacyApp.Application.Interfaces.Services;

namespace PharmacyApp.Presentation.Controllers.Admin;

[ApiController]
[Area("Admin")]
[Route("admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public AdminReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllReviews([FromQuery] ReviewQueryParams queryParams)
    {
        var reviews = await _reviewService.GetAllReviewsAsync(queryParams);

        return Ok(reviews);
    }
    [HttpPost("{reviewId}/approve")]
    public async Task<IActionResult> ApproveReview(int reviewId)
    {
        var result = await _reviewService.ApproveReviewAsync(reviewId);
        if (result)
            return Ok(new { message = "Review approved successfully." });

        return BadRequest(new { message = "Failed to approve review." });
    }

    [HttpPost("{reviewId}/reject")]
    public async Task<IActionResult> RejectReview(int reviewId)
    {
        var result = await _reviewService.RejectReviewAsync(reviewId);

        if (!result)
            return NotFound(new { message = "Review not found." });

        return NoContent();
    }
}
