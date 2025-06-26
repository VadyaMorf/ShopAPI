using Shop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Core.Abstractions
{
    public interface IProductRepository
    {
        Task<long> Create(Product product);
        Task<long> Delete(long id);
        Task<List<Product>> Get();
        Task<List<Product>> GetByCategory(string category);
        Task<long> Update(long id, Product product);
    }
}
