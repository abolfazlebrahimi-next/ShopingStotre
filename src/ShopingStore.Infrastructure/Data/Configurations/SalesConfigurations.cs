using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopingStore.Domain.Entities;

namespace ShopingStore.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.Property(c => c.CartKey).HasMaxLength(64).IsRequired();
        builder.Property(c => c.DiscountCode).HasMaxLength(60);

        builder.HasIndex(c => c.CartKey);
        builder.HasIndex(c => c.UserId);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart!)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.Property(c => c.VariantSelection).HasMaxLength(250);

        builder.HasIndex(c => new { c.CartId, c.ProductId });

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");
        builder.HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();

        builder.HasOne(f => f.Product)
            .WithMany()
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.Property(o => o.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(o => o.CustomerName).HasMaxLength(150).IsRequired();
        builder.Property(o => o.CustomerPhone).HasMaxLength(20).IsRequired();
        builder.Property(o => o.CustomerEmail).HasMaxLength(200);
        builder.Property(o => o.Province).HasMaxLength(100);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.PostalCode).HasMaxLength(20);
        builder.Property(o => o.AddressLine).HasMaxLength(600);
        builder.Property(o => o.DiscountCode).HasMaxLength(60);
        builder.Property(o => o.PaymentReference).HasMaxLength(120);
        builder.Property(o => o.TrackingCode).HasMaxLength(120);
        builder.Property(o => o.CustomerNote).HasMaxLength(1000);
        builder.Property(o => o.AdminNote).HasMaxLength(1000);

        builder.Property(o => o.Subtotal).HasPrecision(18, 0);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 0);
        builder.Property(o => o.ShippingCost).HasPrecision(18, 0);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 0);
        builder.Property(o => o.Total).HasPrecision(18, 0);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => new { o.Status, o.PaymentStatus });
        builder.HasIndex(o => o.CreatedAtUtc);

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.Property(i => i.ProductName).HasMaxLength(250).IsRequired();
        builder.Property(i => i.ProductImageUrl).HasMaxLength(500);
        builder.Property(i => i.VariantSelection).HasMaxLength(250);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 0);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");
        builder.Property(h => h.Note).HasMaxLength(500);
        builder.Property(h => h.Actor).HasMaxLength(150);

        builder.HasOne(h => h.Order)
            .WithMany(o => o.History)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.ToTable("DiscountCodes");
        builder.Property(d => d.Code).HasMaxLength(60).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(400);
        builder.Property(d => d.Amount).HasPrecision(18, 2);
        builder.Property(d => d.MinOrderAmount).HasPrecision(18, 0);
        builder.Property(d => d.MaxDiscountAmount).HasPrecision(18, 0);

        builder.HasIndex(d => d.Code).IsUnique();
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
