using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Web.Infrastructure;

namespace ShopingStore.Web.Pages.Admin.Products;

/// <summary>فرم محصول در پنل مدیریت (شامل ویژگی‌ها، تنوع‌ها و تصویر).</summary>
public class ProductForm
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public string? MainImageUrl { get; set; }
    public List<string> SpecNames { get; set; } = new();
    public List<string> SpecValues { get; set; } = new();
    public List<string> VariantNames { get; set; } = new();
    public List<string> VariantOptions { get; set; } = new();
}

public class EditModel : PageModel
{
    private readonly IAdminProductService _products;
    private readonly IAdminCategoryService _categories;
    private readonly IFileStorage _storage;
    private readonly ToastService _toast;

    public EditModel(IAdminProductService products, IAdminCategoryService categories, IFileStorage storage, ToastService toast)
    {
        _products = products;
        _categories = categories;
        _storage = storage;
        _toast = toast;
    }

    public Product? Product { get; private set; }
    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();

    [BindProperty]
    public ProductForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken)
    {
        Categories = await _categories.GetAllAsync(cancellationToken);

        if (id is > 0)
        {
            Product = await _products.GetForEditAsync(id.Value, cancellationToken);
            if (Product is null) return NotFound();

            Form = new ProductForm
            {
                Id = Product.Id,
                Name = Product.Name,
                Slug = Product.Slug,
                ShortDescription = Product.ShortDescription,
                Description = Product.Description,
                Brand = Product.Brand,
                Sku = Product.Sku,
                Price = Product.Price,
                DiscountPrice = Product.DiscountPrice,
                Stock = Product.Stock,
                CategoryId = Product.CategoryId,
                IsActive = Product.IsActive,
                IsFeatured = Product.IsFeatured,
                IsNew = Product.IsNew,
                MainImageUrl = Product.MainImageUrl,
                SpecNames = Product.Specifications.Select(s => s.Name).ToList(),
                SpecValues = Product.Specifications.Select(s => s.Value).ToList(),
                VariantNames = Product.Variants.Select(v => v.Name).ToList(),
                VariantOptions = Product.Variants.Select(v => string.Join(", ", v.Options)).ToList()
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Categories = await _categories.GetAllAsync(cancellationToken);

        var specifications = new List<ProductSpecificationDto>();
        for (var i = 0; i < Form.SpecNames.Count; i++)
        {
            var name = Form.SpecNames[i];
            if (string.IsNullOrWhiteSpace(name)) continue;
            specifications.Add(new ProductSpecificationDto
            {
                Name = name,
                Value = i < Form.SpecValues.Count ? Form.SpecValues[i] : string.Empty
            });
        }

        var variants = new List<ProductVariantDto>();
        for (var i = 0; i < Form.VariantNames.Count; i++)
        {
            var name = Form.VariantNames[i];
            if (string.IsNullOrWhiteSpace(name)) continue;

            var options = (i < Form.VariantOptions.Count ? Form.VariantOptions[i] : string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            variants.Add(new ProductVariantDto { Name = name, Options = options.ToList() });
        }

        try
        {
            var productId = await _products.SaveAsync(new SaveProductRequest
            {
                Id = Form.Id,
                Name = Form.Name,
                Slug = Form.Slug,
                ShortDescription = Form.ShortDescription,
                Description = Form.Description,
                Brand = Form.Brand,
                Sku = Form.Sku,
                Price = Form.Price,
                DiscountPrice = Form.DiscountPrice is > 0 ? Form.DiscountPrice : null,
                Stock = Form.Stock,
                CategoryId = Form.CategoryId,
                IsActive = Form.IsActive,
                IsFeatured = Form.IsFeatured,
                IsNew = Form.IsNew,
                MainImageUrl = Form.MainImageUrl,
                Specifications = specifications,
                Variants = variants
            }, cancellationToken);

            _toast.Success("محصول با موفقیت ذخیره شد.");
            return Redirect($"/admin/products/edit/{productId}");
        }
        catch (BusinessException exception)
        {
            _toast.Error(exception.Message);
            Product = Form.Id is > 0 ? await _products.GetForEditAsync(Form.Id.Value, cancellationToken) : null;
            return Page();
        }
    }

    /// <summary>بارگذاری تصویر محصول (درخواست AJAX از صفحه ویرایش).</summary>
    public async Task<IActionResult> OnPostUploadImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return new JsonResult(new { success = false, message = "فایلی انتخاب نشده است." });

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storage.SaveImageAsync(stream, file.FileName, "products", cancellationToken);
            return new JsonResult(new { success = true, url });
        }
        catch (BusinessException exception)
        {
            return new JsonResult(new { success = false, message = exception.Message });
        }
    }
}
