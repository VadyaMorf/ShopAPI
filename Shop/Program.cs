using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces.Auth;
using Shop.Application.Services;
using Shop.Core.Abstractions;
using Shop.DataAccess;
using Shop.DataAccess.Repositiries;
using Shop.Endpoints;
using Shop.Infastracture;
using AutoMapper;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Добавляем поддержку переменных окружения
builder.Configuration.AddEnvironmentVariables();

// --- Railway/Render: поддержка переменной PORT ---
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}
// --------------------------------------------------

builder.Services.AddAutoMapper(typeof(Program));
services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- ВОЗВРАЩАЮ ПОДКЛЮЧЕНИЕ К БД ---
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
builder.Services.AddDbContext<ShopDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
// -----------------------------------

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepositories, UserRepositories>();
builder.Services.AddScoped<ProductXmlImportService>();
builder.Services.AddScoped<TonometerXmlService>();

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

// --- ВОЗВРАЩАЮ ИМПОРТ ДАННЫХ ПРИ СТАРТЕ ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    var importService = scope.ServiceProvider.GetRequiredService<ProductXmlImportService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Проверяем, есть ли продукты в базе данных
    var productsCount = await context.Products.CountAsync();
    if (productsCount == 0)
    {
        try
        {
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "xml_files", "products_feed.xml"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "products_feed.xml"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "products_feed.xml"),
                Path.Combine(Directory.GetCurrentDirectory(), "products_feed.xml")
            };
            string? xmlPath = null;
            foreach (var path in possiblePaths)
            {
                logger.LogInformation("Проверяем путь: {Path}", path);
                if (File.Exists(path))
                {
                    xmlPath = path;
                    logger.LogInformation("XML файл найден по пути: {Path}", path);
                    break;
                }
            }
            if (xmlPath != null)
            {
                logger.LogInformation("Начинаем импорт данных из XML");
                await importService.ImportAll(xmlPath);
                logger.LogInformation("Данные успешно импортированы из XML файла");
            }
            else
            {
                logger.LogWarning("XML файл не найден ни по одному из путей");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при импорте данных: {Message}", ex.Message);
        }
    }
    else
    {
        logger.LogInformation("В базе данных уже есть {ProductsCount} продуктов", productsCount);
    }
}
// --------------------------------------------

app.Run();
