using Shop.Application.Interfaces.Auth;
using Shop.Application.Interfaces.Repositories;
using Shop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Application.Services
{
    public class UserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepositories _repository;
        private readonly IJWTProvider _provider;
        public UserService(IUserRepositories repository, IPasswordHasher passwordHasher, IJWTProvider provider)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _provider = provider;
        }
        public async Task Register(string userName, string email, string password)
        {
            var hashPassword = _passwordHasher.Generate(password);

            var user = User.Create(Guid.NewGuid(), userName, email, hashPassword);

            await _repository.Add(user);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _repository.GetByEmail(email);

            var result = _passwordHasher.Verify(password, user.PasswordHash);

            if (result == false)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = _provider.GenerateToken(user);
            return token;
        }
    }
}
