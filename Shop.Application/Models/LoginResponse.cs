namespace Shop.Application.Models
{
    public record LoginResponse(
        string Token,
        Guid UserId,
        string UserName,
        string Email,
        string FirstName,
        string LastName,
        string PhoneNumber);
} 