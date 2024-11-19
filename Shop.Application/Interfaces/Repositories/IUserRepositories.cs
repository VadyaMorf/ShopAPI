using Shop.Core.Models;

namespace Shop.Application.Interfaces.Repositories
{
    public interface IUserRepositories
    {
        Task Add(User user);
        Task<User> GetByEmail(string email);
    }
}