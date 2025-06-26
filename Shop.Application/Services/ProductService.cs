using Shop.Core.Abstractions;

namespace Shop.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly TonometerXmlService _tonometerXmlService;
        
        public ProductService(IProductRepository productRepository, TonometerXmlService tonometerXmlService)
        {
            _repository = productRepository;
            _tonometerXmlService = tonometerXmlService;
        }

        public async Task<List<Shop.Core.Models.Product>> GetAllProducts()
        {
            return await _repository.Get();
        }

        public async Task<List<Shop.Core.Models.Product>> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return await _repository.Get();
            
            // Специальная обработка для тонометров
            if (category.Equals("tonometrs", StringComparison.OrdinalIgnoreCase))
            {
                var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "tonometrs_catalog.xml");
                return await _tonometerXmlService.GetTonometersFromXml(xmlPath);
            }
                
            return await _repository.GetByCategory(category);
        }

        public async Task<long> CreateProduct(Shop.Core.Models.Product product)
        {
            return await _repository.Create(product);
        }

        public async Task<long> UpdateProduct(long id, Shop.Core.Models.Product product)
        {
            return await _repository.Update(id, product);
        }
        public async Task<long> DeleteProduct(long id)
        {
            return await _repository.Delete(id);
        }
    }
}
