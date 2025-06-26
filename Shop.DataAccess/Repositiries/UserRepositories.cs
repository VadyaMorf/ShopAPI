using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shop.Core.Models;
using Shop.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shop.Core.Abstractions;

namespace Shop.DataAccess.Repositiries
{
    public class UserRepositories : IUserRepositories
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;
        public UserRepositories(ShopDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task Add(User user)
        {
            var userEntity = new UserEntity()
            {
                Id = user.Id,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
                Email = user.Email
            };

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByEmail(string email)
        {
            var userEntity = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email) ?? throw new Exception();

            return _mapper.Map<User>(userEntity);
        }
    }
}
