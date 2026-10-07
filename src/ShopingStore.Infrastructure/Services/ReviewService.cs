using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly IProductRepository _products;
    private readonly IUserRepository _users;
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _uow;

    public ReviewService(
        IReviewRepository reviews,
        IProductRepository products,
        IUserRepository users,
        IOrderService orderService,
        IUnitOfWork uow)
    {
        _reviews = reviews;
        _products = products;
        _users = users;
        _orderService = orderService;
        _uow = uow;
    }

    public async Task<ReviewDto> AddAsync(int userId, AddReviewRequest request, CancellationToken ct = default)
    {
        Guard.InRange(request.Rating, 1, 5, "امتیاز");
        var comment = Guard.NotEmpty(request.Comment, "متن نظر", 2000);

        var product = await _products.GetByIdAsync(request.ProductId, false, ct)
            ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

        if (await _reviews.HasUserReviewedAsync(request.ProductId, userId, ct))
            throw new BusinessException("شما قبلاً برای این محصول نظر ثبت کرده‌اید.");

        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("کاربر یافت نشد.");

        var review = new Review
        {
            ProductId = product.Id,
            UserId = user.Id,
            AuthorName = user.FullName,
            Title = Guard.Optional(request.Title, 200),
            Comment = comment,
            Rating = request.Rating,
            Status = ReviewStatus.Pending,
            IsVerifiedPurchase = await _orderService.CanUserReviewProductAsync(userId, product.Id, ct)
        };

        await _reviews.AddAsync(review, ct);
        await _uow.SaveChangesAsync(ct);

        review.Product = product;
        return review.ToDto();
    }

    public async Task<PagedResult<ReviewDto>> SearchAsync(string? search, ReviewStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _reviews.SearchAsync(search, status, page, pageSize, ct);
        return result.Map(r => r.ToDto());
    }

    public async Task ApproveAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(reviewId, ct) ?? throw new NotFoundException("نظر مورد نظر یافت نشد.");
        review.Status = ReviewStatus.Approved;
        _reviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        await RefreshProductRatingAsync(review.ProductId, ct);
    }

    public async Task RejectAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(reviewId, ct) ?? throw new NotFoundException("نظر مورد نظر یافت نشد.");
        review.Status = ReviewStatus.Rejected;
        _reviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        await RefreshProductRatingAsync(review.ProductId, ct);
    }

    public async Task DeleteAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(reviewId, ct) ?? throw new NotFoundException("نظر مورد نظر یافت نشد.");
        var productId = review.ProductId;

        _reviews.Remove(review);
        await _uow.SaveChangesAsync(ct);

        await RefreshProductRatingAsync(productId, ct);
    }

    private async Task RefreshProductRatingAsync(int productId, CancellationToken ct)
    {
        var (average, count) = await _reviews.GetRatingAsync(productId, ct);

        var product = await _products.GetByIdAsync(productId, false, ct);
        if (product is null) return;

        product.Rating = average;
        product.ReviewsCount = count;
        _products.Update(product);

        await _uow.SaveChangesAsync(ct);
    }
}
