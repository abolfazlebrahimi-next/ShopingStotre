using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(160).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.Icon).HasMaxLength(500);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Name);

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(260).IsRequired();
        builder.Property(p => p.ShortDescription).HasMaxLength(600);
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");
        builder.Property(p => p.Brand).HasMaxLength(120);
        builder.Property(p => p.Sku).HasMaxLength(60);
        builder.Property(p => p.MainImageUrl).HasMaxLength(500);

        builder.Property(p => p.Price).HasPrecision(18, 0);
        builder.Property(p => p.DiscountPrice).HasPrecision(18, 0);
        builder.Property(p => p.Rating).HasPrecision(3, 2);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ویژگی‌های فنی و تنوع‌ها به شکل ستون JSON ذخیره می‌شوند
        builder.OwnsMany(p => p.Specifications, owned =>
        {
            owned.ToJson("Specifications");
            owned.Property(s => s.Name).HasMaxLength(150);
            owned.Property(s => s.Value).HasMaxLength(400);
        });

        builder.OwnsMany(p => p.Variants, owned =>
        {
            owned.ToJson("Variants");
            owned.Property(v => v.Name).HasMaxLength(100);
            owned.Property(v => v.Options);
        });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.Property(i => i.Url).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Alt).HasMaxLength(250);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.Property(r => r.AuthorName).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(200);
        builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();

        builder.HasIndex(r => new { r.ProductId, r.Status });

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
