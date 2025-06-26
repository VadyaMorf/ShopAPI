using Microsoft.EntityFrameworkCore;
using Shop.Core.Abstractions;
using Shop.Core.Models;
using Shop.DataAccess.Entities;
using System.Text.Json;

namespace Shop.DataAccess.Repositiries
{
    public class ProductRepository : IProductRepository
    {
        private readonly ShopDbContext _context;
        public ProductRepository(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> Get()
        {
            var productEntities = await _context.Products.AsNoTracking().ToListAsync();
            var products = productEntities.Select(b => new Product
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Price = b.Price,
                Vendor = b.Vendor,
                CountryOfOrigin = b.CountryOfOrigin,
                Url = b.Url,
                CategoryId = b.CategoryId,
                Category = b.Category,
                CurrencyId = b.CurrencyId,
                Pictures = string.IsNullOrEmpty(b.Pictures) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(b.Pictures),
                Available = b.Available,
                Params = string.IsNullOrEmpty(b.Params) ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(b.Params)
            }).ToList();
            return products;
        }

        public async Task<List<Product>> GetByCategory(string category)
        {
            var productEntities = await _context.Products
                .AsNoTracking()
                .Where(p => p.Category.ToLower().Contains(category.ToLower()))
                .ToListAsync();
                
            var products = productEntities.Select(b => new Product
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Price = b.Price,
                Vendor = b.Vendor,
                CountryOfOrigin = b.CountryOfOrigin,
                Url = b.Url,
                CategoryId = b.CategoryId,
                Category = b.Category,
                CurrencyId = b.CurrencyId,
                Pictures = string.IsNullOrEmpty(b.Pictures) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(b.Pictures),
                Available = b.Available,
                Params = string.IsNullOrEmpty(b.Params) ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(b.Params)
            }).ToList();
            return products;
        }

        public async Task<long> Create(Product product)
        {
            var productEntity = new ProductEntity
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Vendor = product.Vendor,
                CountryOfOrigin = product.CountryOfOrigin,
                Url = product.Url,
                CategoryId = product.CategoryId,
                Category = product.Category,
                CurrencyId = product.CurrencyId,
                Pictures = JsonSerializer.Serialize(product.Pictures),
                Available = product.Available,
                Params = JsonSerializer.Serialize(product.Params)
            };
            await _context.Products.AddAsync(productEntity);
            await _context.SaveChangesAsync();
            return productEntity.Id;
        }

        public async Task<long> Update(long id, Product product)
        {
            var entity = await _context.Products.FirstOrDefaultAsync(b => b.Id == id);
            if (entity == null) return 0;
            entity.Name = product.Name;
            entity.Description = product.Description;
            entity.Price = product.Price;
            entity.Vendor = product.Vendor;
            entity.CountryOfOrigin = product.CountryOfOrigin;
            entity.Url = product.Url;
            entity.CategoryId = product.CategoryId;
            entity.Category = product.Category;
            entity.CurrencyId = product.CurrencyId;
            entity.Pictures = JsonSerializer.Serialize(product.Pictures);
            entity.Available = product.Available;
            entity.Params = JsonSerializer.Serialize(product.Params);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<long> Delete(long id)
        {
            var entity = await _context.Products.FirstOrDefaultAsync(b => b.Id == id);
            if (entity == null) return 0;
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            return id;
        }
    }
}
