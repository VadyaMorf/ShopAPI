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
            builder.Property(b => b.Title).IsRequired().HasMaxLength(Product.Max_Titile_Lenght);
            builder.Property(b => b.Description).IsRequired();
            builder.Property(b => b.Price).IsRequired();
            builder.Property(b => b.Count).IsRequired();
        }
    }
}
