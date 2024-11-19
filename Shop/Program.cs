using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces.Auth;
using Shop.Application.Interfaces.Repositories;
using Shop.Application.Services;
using Shop.Core.Abstractions;
using Shop.DataAccess;
using Shop.DataAccess.Repositiries;
using Shop.Endpoints;
using Shop.Infastracture;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Services.AddAutoMapper(typeof(Program));
services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShopDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ShopDbContext)));
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepositories, UserRepositories>();

builder.Services.AddScoped<UserService>();


builder.Services.AddScoped<IJWTProvider, JWTProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.MapUsersEndpoints();

app.UseCors(x =>
{
    x.AllowAnyHeader();      
    x.AllowAnyMethod();     
    x.AllowAnyOrigin();     
});


app.Run();
