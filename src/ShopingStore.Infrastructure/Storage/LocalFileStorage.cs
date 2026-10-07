using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopingStore.Domain.Common;
using ShopingStore.Domain.Interfaces.Repositories;

namespace ShopingStore.Infrastructure.Storage;

public class StorageOptions
{
    /// <summary>مسیر فیزیکی پوشه آپلود (مثلاً wwwroot/uploads).</summary>
    public string RootPath { get; set; } = "wwwroot/uploads";

    /// <summary>پیشوند آدرس عمومی فایل‌ها.</summary>
    public string PublicBasePath { get; set; } = "/uploads";

    public long MaxFileSizeBytes { get; set; } = 3 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
}

/// <summary>ذخیره‌سازی تصاویر روی فایل‌سیستم محلی سرور.</summary>
public class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<StorageOptions> options, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(Stream content, string fileName, string folder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_options.AllowedExtensions.Contains(extension))
            throw new BusinessException("فرمت تصویر مجاز نیست. فرمت‌های مجاز: " + string.Join("، ", _options.AllowedExtensions));

        if (content.CanSeek && content.Length > _options.MaxFileSizeBytes)
            throw new BusinessException($"حجم تصویر نباید بیشتر از {_options.MaxFileSizeBytes / 1024 / 1024} مگابایت باشد.");

        var safeFolder = string.Join('/', folder.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries));
        var directory = Path.Combine(_options.RootPath, safeFolder);
        Directory.CreateDirectory(directory);

        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, uniqueName);

        await using (var file = File.Create(fullPath))
        {
            await content.CopyToAsync(file, ct);
        }

        _logger.LogInformation("تصویر در {Path} ذخیره شد.", fullPath);

        return $"{_options.PublicBasePath}/{safeFolder}/{uniqueName}".Replace("//", "/");
    }

    public Task DeleteAsync(string relativeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return Task.CompletedTask;

        var relative = relativeUrl.StartsWith(_options.PublicBasePath, StringComparison.OrdinalIgnoreCase)
            ? relativeUrl[_options.PublicBasePath.Length..]
            : relativeUrl;

        var fullPath = Path.Combine(_options.RootPath, relative.TrimStart('/', '\\').Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(fullPath)) File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
