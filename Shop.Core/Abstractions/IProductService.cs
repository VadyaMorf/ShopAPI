using Shop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Core.Abstractions
{
    public interface IProductService
    {
        Task<long> CreateProduct(Product product);
        Task<long> DeleteProduct(long id);
        Task<List<Product>> GetAllProducts();
        Task<List<Product>> GetProductsByCategory(string category);
        Task<long> UpdateProduct(long id, Product product);
    }
}
