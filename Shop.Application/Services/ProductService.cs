using Shop.Core.Abstractions;

namespace Shop.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        public ProductService(IProductRepository productRepository)
        {
            _repository = productRepository;
        }

        public async Task<List<Shop.Core.Models.Product>> GetAllProducts()
        {
            return await _repository.Get();
        }

        public async Task<Guid> CreateProduct(Shop.Core.Models.Product product)
        {
            return await _repository.Create(product);
        }

        public async Task<Guid> UpdateProduct(Guid id, string title, string description, decimal price,  decimal count)
        {
            return await _repository.Update(id, title, description, price, count);
        }
        public async Task<Guid> DeleteProduct(Guid id)
        {
            return await _repository.Delete(id);
        }
    }
}
