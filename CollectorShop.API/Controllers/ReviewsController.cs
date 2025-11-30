using System.Security.Claims;
using CollectorShop.API.DTOs.Reviews;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IUnitOfWork unitOfWork, ILogger<ReviewsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(Guid productId)
    {
        var reviews = await _unitOfWork.Reviews.GetApprovedReviewsAsync(productId);

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? string.Empty,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName ?? "Anonymous",
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt
        });

        return Ok(reviewDtos);
    }

    [HttpGet("product/{productId:guid}/stats")]
    public async Task<ActionResult> GetProductReviewStats(Guid productId)
    {
        var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);
        var reviewCount = await _unitOfWork.Reviews.CountAsync(r => r.ProductId == productId && r.IsApproved);

        return Ok(new { AverageRating = averageRating, ReviewCount = reviewCount });
    }

    [Authorize]
    [HttpGet("my-reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMyReviews()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var reviews = await _unitOfWork.Reviews.GetByCustomerIdAsync(customerId.Value);

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? string.Empty,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName ?? "Anonymous",
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt
        });

        return Ok(reviewDtos);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return NotFound(new { Message = "Product not found" });
        }

        var hasReviewed = await _unitOfWork.Reviews.HasCustomerReviewedProductAsync(customerId.Value, request.ProductId);
        if (hasReviewed)
        {
            return BadRequest(new { Message = "You have already reviewed this product" });
        }

        var review = new Review(
            request.ProductId,
            customerId.Value,
            request.Rating,
            request.Title,
            request.Comment
        );

        // Check if customer has purchased this product
        var customerOrders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId.Value);
        var hasPurchased = customerOrders.Any(o =>
            o.Items.Any(i => i.ProductId == request.ProductId) &&
            (o.Status == Domain.Enums.OrderStatus.Delivered || o.Status == Domain.Enums.OrderStatus.Shipped));

        if (hasPurchased)
        {
            review.MarkAsVerifiedPurchase();
        }

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review created for product {ProductId} by customer {CustomerId}", request.ProductId, customerId);

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId.Value);

        return CreatedAtAction(nameof(GetProductReviews), new { productId = request.ProductId }, new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = product.Name,
            CustomerId = review.CustomerId,
            CustomerName = customer?.FullName ?? "Anonymous",
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt
        });
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.CustomerId != customerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        review.UpdateReview(request.Rating, request.Title, request.Comment);

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} updated", id);

        return Ok(new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            CustomerId = review.CustomerId,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt
        });
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);

        if (review == null)
        {
            return NotFound();
        }

        if (review.CustomerId != customerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} deleted", id);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetPendingReviews()
    {
        var reviews = await _unitOfWork.Reviews.GetPendingReviewsAsync();

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? string.Empty,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName ?? "Anonymous",
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt
        });

        return Ok(reviewDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveReview(Guid id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        review.Approve();

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} approved", id);

        return Ok(new { Message = "Review approved successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectReview(Guid id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        review.Reject();

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} rejected", id);

        return Ok(new { Message = "Review rejected successfully" });
    }

    private async Task<Guid?> GetCurrentCustomerIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        return customer?.Id;
    }
}
