using ShopingStore.Domain.Common;
using ShopingStore.Domain.Dtos;
using ShopingStore.Domain.Entities;
using ShopingStore.Domain.Interfaces.Repositories;
using ShopingStore.Domain.Interfaces.Services;
using ShopingStore.Infrastructure.Mapping;

namespace ShopingStore.Infrastructure.Services;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly IDiscountCodeRepository _codes;
    private readonly IUnitOfWork _uow;

    public DiscountCodeService(IDiscountCodeRepository codes, IUnitOfWork uow)
    {
        _codes = codes;
        _uow = uow;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var codes = await _codes.GetAllAsync(ct);
        return codes.Select(c => c.ToDto()).ToList();
    }

    public Task<DiscountCode?> GetForEditAsync(int id, CancellationToken ct = default) => _codes.GetByIdAsync(id, ct);

    public async Task<int> SaveAsync(SaveDiscountCodeRequest request, CancellationToken ct = default)
    {
        var code = Guard.NotEmpty(request.Code, "کد تخفیف", 60).ToUpperInvariant().Replace(" ", string.Empty);

        if (request.Type == DiscountType.Percentage && request.Amount is <= 0 or > 100)
            throw new BusinessException("درصد تخفیف باید بین ۱ تا ۱۰۰ باشد.");

        if (request.Type == DiscountType.FixedAmount)
            Guard.Positive(request.Amount, "مبلغ تخفیف");

        Guard.NotNegative(request.MinOrderAmount, "حداقل مبلغ سفارش");

        if (request.ExpiresAtUtc is not null && request.StartsAtUtc is not null && request.ExpiresAtUtc < request.StartsAtUtc)
            throw new BusinessException("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");

        DiscountCode entity;
        if (request.Id is > 0)
        {
            entity = await _codes.GetByIdAsync(request.Id.Value, ct) ?? throw new NotFoundException("کد تخفیف مورد نظر یافت نشد.");
        }
        else
        {
            entity = new DiscountCode();
            await _codes.AddAsync(entity, ct);
        }

        if (await _codes.CodeExistsAsync(code, entity.Id == 0 ? null : entity.Id, ct))
            throw new BusinessException("این کد تخفیف قبلاً ثبت شده است.");

        entity.Code = code;
        entity.Description = Guard.Optional(request.Description, 400);
        entity.Type = request.Type;
        entity.Amount = request.Amount;
        entity.MinOrderAmount = request.MinOrderAmount;
        entity.MaxDiscountAmount = request.MaxDiscountAmount;
        entity.StartsAtUtc = request.StartsAtUtc;
        entity.ExpiresAtUtc = request.ExpiresAtUtc;
        entity.UsageLimit = request.UsageLimit;
        entity.IsActive = request.IsActive;

        _codes.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _codes.GetByIdAsync(id, ct) ?? throw new NotFoundException("کد تخفیف مورد نظر یافت نشد.");
        _codes.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<(bool Success, string? Message, decimal Amount)> ValidateAsync(string code, decimal cartSubtotal, CancellationToken ct = default)
    {
        var entity = await _codes.GetByCodeAsync(code ?? string.Empty, ct);
        if (entity is null) return (false, "کد تخفیف وارد‌شده معتبر نیست.", 0);

        if (!entity.IsUsable(DateTime.UtcNow, cartSubtotal, out var error))
            return (false, error, 0);

        return (true, null, entity.CalculateDiscount(cartSubtotal));
    }
}
