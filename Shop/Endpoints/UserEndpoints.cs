using Shop.Application.Services;
using Shop.Contracts;

namespace Shop.Endpoints
{
    public static class UsersEndpoints
    {
        public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("register", Register);
            builder.MapPost("login", Login);
            return builder;
        }

        private static async Task<IResult> Register(RegisterUserRequest registerUserRequest, UserService userService)
        {
            await userService.Register(registerUserRequest.UserName, registerUserRequest.Email, registerUserRequest.Password);

            return Results.Ok();
        }
        private static async Task<IResult> Login(LoginRequest request, UserService userService, HttpContext context)
        {
            var token = await userService.Login(request.Email, request.Password);

            context.Response.Cookies.Append("user-cookies", token);

            return Results.Ok();
        }
    }
}
