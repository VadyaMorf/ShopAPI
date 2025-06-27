using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Core.Models
{
    public class User
    {
        private User(Guid id, string userName, string passwordHash, string email, string firstName, string lastName, string phoneNumber)
        {
            Id = id;
            UserName = userName;
            PasswordHash = passwordHash;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }

        public Guid Id { get; set; }
        public string UserName { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PhoneNumber { get; private set; }

        public static User Create(Guid id, string userName, string passwordHash, string email, string firstName, string lastName, string phoneNumber)
        {
            return new User(id, userName, passwordHash, email, firstName, lastName, phoneNumber); 
        }
    }
}
