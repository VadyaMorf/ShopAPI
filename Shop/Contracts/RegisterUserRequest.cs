using System.ComponentModel.DataAnnotations;

namespace Shop.Contracts
{
    public record RegisterUserRequest(
        [Required] string UserName,
        [Required] string Password,
        [Required] string Email);
}
