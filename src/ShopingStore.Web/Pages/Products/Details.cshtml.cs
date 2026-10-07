using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Interfaces;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly ICatalogService _catalog;
    private readonly IReviewService _reviews;
    private readonly ICurrentUser _currentUser;
    private readonly ToastService _toast;

    public DetailsModel(ICatalogService catalog, IReviewService reviews, ICurrentUser currentUser, ToastService toast)
    {
        _catalog = catalog;
        _reviews = reviews;
        _currentUser = currentUser;
        _toast = toast;
    }

    public ProductDetailsDto Product { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await _catalog.GetProductDetailsAsync(slug, _currentUser.UserId, true, cancellationToken);
        if (product is null) return NotFound();

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, int rating, string? title, string comment, CancellationToken cancellationToken)
    {
        var product = await _catalog.GetProductDetailsAsync(slug, _currentUser.UserId, false, cancellationToken);
        if (product is null) return NotFound();

        if (_currentUser.UserId is null) return Redirect($"/account/login?returnUrl=/product/{slug}");

        try
        {
            await _reviews.AddAsync(_currentUser.UserId.Value, new AddReviewRequest
            {
                ProductId = product.Id,
                Rating = rating,
                Title = title,
                Comment = comment
            }, cancellationToken);

            _toast.Success("نظر شما ثبت شد و پس از تأیید نمایش داده می‌شود.");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
        }

        return Redirect($"/product/{slug}");
    }
}
