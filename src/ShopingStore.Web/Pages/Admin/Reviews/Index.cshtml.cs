using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Reviews;

public class IndexModel : PageModel
{
    private readonly IReviewService _reviews;
    private readonly ToastService _toast;

    public IndexModel(IReviewService reviews, ToastService toast)
    {
        _reviews = reviews;
        _toast = toast;
    }

    public PagedResult<ReviewDto> Reviews { get; private set; } = PagedResult<ReviewDto>.Empty();
    public string? Search { get; private set; }
    public ReviewStatus? Status { get; private set; }
    public PaginationModel Pagination { get; private set; } = new();

    public async Task OnGetAsync(string? search, ReviewStatus? status, int page, CancellationToken cancellationToken)
    {
        Search = search;
        Status = status;

        Reviews = await _reviews.SearchAsync(search, status, page <= 0 ? 1 : page, 15, cancellationToken);

        Pagination = new PaginationModel
        {
            Page = Reviews.Page,
            TotalPages = Reviews.TotalPages,
            TotalCount = Reviews.TotalCount,
            BasePath = "/admin/reviews",
            Query = Request.Query
        };
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, CancellationToken cancellationToken)
    {
        await _reviews.ApproveAsync(id, cancellationToken);
        _toast.Success("نظر تأیید و در سایت نمایش داده می‌شود.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, CancellationToken cancellationToken)
    {
        await _reviews.RejectAsync(id, cancellationToken);
        _toast.Info("نظر رد شد.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _reviews.DeleteAsync(id, cancellationToken);
        _toast.Success("نظر حذف شد.");
        return RedirectToPage();
    }
}
