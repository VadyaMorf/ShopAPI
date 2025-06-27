using Shop.Core.Models;
using System.Threading.Tasks;

namespace Shop.Core.Abstractions
{
    public interface IUserRepositories
    {
        Task Add(User user);
        Task<User> GetByEmail(string email);
        Task<User?> GetByIdAsync(int id);
    }
} 