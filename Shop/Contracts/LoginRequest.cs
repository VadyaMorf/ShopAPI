using System.ComponentModel.DataAnnotations;

namespace Shop.Contracts
{
    public record LoginRequest(
        [Required] string Email,
        [Required] string Password);
}
