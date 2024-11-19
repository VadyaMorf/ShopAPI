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
        Task<Guid> CreateProduct(Product product);
        Task<Guid> DeleteProduct(Guid id);
        Task<List<Product>> GetAllProducts();
        Task<Guid> UpdateProduct(Guid id, string title, string description, decimal price, decimal count);
    }
}
