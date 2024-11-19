using Microsoft.EntityFrameworkCore;
using Shop.Core.Abstractions;
using Shop.Core.Models;
using Shop.DataAccess.Entities;


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
            var productEntities = await EntityFrameworkQueryableExtensions
                .AsNoTracking(_context.Products)
                .ToListAsync();

            var products = productEntities.Select(b => Product.Create(b.Id, b.Title, b.Description, b.Price, b.Count).Product).ToList();

            return products;
        }

        public async Task<Guid> Create(Product product)
        {
            var productEntity = new ProductEntity
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Count = product.Count
            };
            await _context.Products.AddAsync(productEntity);
            await _context.SaveChangesAsync();

            return productEntity.Id;
        }

        public async Task<Guid> Update(Guid id, string title, string descriprion, decimal price, decimal count)
        {
            await _context.Products.Where(b => b.Id == id).
                ExecuteUpdateAsync(s => s.
                SetProperty(b => b.Title, b => title).
                SetProperty(b => b.Description, b => descriprion).
                SetProperty(b => b.Price, b => price).
                SetProperty(b => b.Count, b => count));

            return id;
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _context.Products.Where(b => b.Id == id).ExecuteDeleteAsync();

            return id;
        }
    }
}
