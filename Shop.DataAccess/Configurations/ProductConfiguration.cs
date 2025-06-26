using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shop.DataAccess.Entities;
using Shop.Core.Models;

namespace Shop.DataAccess.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
    {
        public void Configure(EntityTypeBuilder<ProductEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(250);
            builder.Property(b => b.Description).IsRequired();
            builder.Property(b => b.Price).IsRequired();
            builder.Property(b => b.Vendor).HasMaxLength(100);
            builder.Property(b => b.CountryOfOrigin).HasMaxLength(100);
            builder.Property(b => b.Url).HasMaxLength(500);
            builder.Property(b => b.CategoryId).IsRequired();
            builder.Property(b => b.CurrencyId).HasMaxLength(10);
            builder.Property(b => b.Pictures).HasColumnType("text");
            builder.Property(b => b.Available).IsRequired();
            builder.Property(b => b.Params).HasColumnType("text");
        }
    }
}
