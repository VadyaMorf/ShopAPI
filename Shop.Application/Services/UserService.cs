using Shop.Application.Interfaces.Auth;
using Shop.Core.Abstractions;
using Shop.Core.Models;
using Shop.Application.Models;
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
        public async Task Register(string userName, string email, string password, string firstName, string lastName, string phoneNumber)
        {
            var hashPassword = _passwordHasher.Generate(password);

            var user = User.Create(Guid.NewGuid(), userName, hashPassword, email, firstName, lastName, phoneNumber);

            await _repository.Add(user);
        }

        public async Task<LoginResponse> Login(string email, string password)
        {
            var user = await _repository.GetByEmail(email);

            var result = _passwordHasher.Verify(password, user.PasswordHash);

            if (result == false)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = _provider.GenerateToken(user);
            
            return new LoginResponse(
                token,
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber);
        }
    }
}
