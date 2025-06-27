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
using Shop.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Добавляем поддержку переменных окружения
builder.Configuration.AddEnvironmentVariables();

// Добавляем поддержку замены переменных окружения в конфигурации
builder.Configuration.AddEnvironmentVariables(prefix: "");

// --- Railway/Render: поддержка переменной PORT ---
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}
// --------------------------------------------------

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(Shop.DataAccess.AutoMapperProfile));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- ВОЗВРАЩАЮ ПОДКЛЮЧЕНИЕ К БД ---
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

// Логируем переменные окружения (без пароля)
var configLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
configLogger.LogInformation("Переменные окружения БД: Host={Host}, Port={Port}, Database={Database}, User={User}", 
    dbHost, dbPort, dbName, dbUser);

string connectionString;
if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
{
    // Используем переменные окружения для Railway/Render
    connectionString = $"Host={dbHost};Port={dbPort ?? "5432"};Database={dbName ?? "shop_db"};Username={dbUser};Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true";
    configLogger.LogInformation("Используем переменные окружения для подключения к БД");
}
else
{
    // Fallback на локальную конфигурацию
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=localhost;Port=5432;Database=shop_db;Username=postgres;Password=password";
    configLogger.LogInformation("Используем локальную конфигурацию для подключения к БД");
}

builder.Services.AddDbContext<ShopDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
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

// Добавляем JWT аутентификацию
var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
// Пытаемся получить секретный ключ из переменной окружения
var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? Environment.GetEnvironmentVariable("JWT_KEY");

// Заменяем placeholder в SecretKey если он есть
if (jwtOptions != null && !string.IsNullOrEmpty(jwtOptions.SecretKey) && jwtOptions.SecretKey.StartsWith("${"))
{
    var placeholderName = jwtOptions.SecretKey.Trim('$', '{', '}');
    var placeholderValue = Environment.GetEnvironmentVariable(placeholderName);
    if (!string.IsNullOrEmpty(placeholderValue))
    {
        jwtOptions.SecretKey = placeholderValue;
        configLogger.LogInformation("Заменён placeholder {Placeholder} на значение из переменной окружения", placeholderName);
    }
}

if (!string.IsNullOrEmpty(envSecret))
{
    jwtOptions.SecretKey = envSecret;
    configLogger.LogInformation("Используем JWT секрет из переменной окружения (длина: {Length})", envSecret.Length);
    configLogger.LogInformation("Первые 10 символов ключа: {KeyStart}", envSecret.Substring(0, Math.Min(10, envSecret.Length)));
    configLogger.LogInformation("Последние 10 символов ключа: {KeyEnd}", envSecret.Substring(Math.Max(0, envSecret.Length - 10)));
    configLogger.LogInformation("Битовый размер ключа: {BitSize}", envSecret.Length * 8);
    
    // Проверяем минимальную длину ключа
    if (envSecret.Length < 32)
    {
        configLogger.LogError("JWT ключ слишком короткий! Минимальная длина: 32 символа, текущая: {Length}", envSecret.Length);
    }
    
    // Проверяем на наличие пробелов или невидимых символов
    if (envSecret.Trim() != envSecret)
    {
        configLogger.LogWarning("JWT ключ содержит пробелы в начале или конце!");
    }
}
else
{
    configLogger.LogWarning("JWT секрет из переменной окружения не найден, используем из appsettings.json (длина: {Length})", jwtOptions.SecretKey.Length);
    if (jwtOptions.SecretKey.Length < 32)
    {
        configLogger.LogError("JWT ключ из appsettings.json слишком короткий! Минимальная длина: 32 символа, текущая: {Length}", jwtOptions.SecretKey.Length);
    }
}
if (jwtOptions != null)
{
    // Регистрируем обновлённую конфигурацию в DI
    services.Configure<JwtOptions>(options =>
    {
        options.SecretKey = jwtOptions.SecretKey;
        options.ExpiresHours = jwtOptions.ExpiresHours;
    });
    
    builder.Services.AddApiAuthentication(Options.Create(jwtOptions));
}

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

// Исправляем порядок middleware
app.UseAuthentication();
app.UseAuthorization();

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
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var importService = scope.ServiceProvider.GetRequiredService<ProductXmlImportService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Проверяем подключение к БД
        logger.LogInformation("Проверяем подключение к базе данных...");
        await context.Database.CanConnectAsync();
        logger.LogInformation("Подключение к базе данных успешно установлено");
        
        // Проверяем, есть ли продукты в базе данных
        var productsCount = await context.Products.CountAsync();
        if (productsCount == 0)
        {
            try
            {
                var possiblePaths = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "xml_files", "products_feed.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "xml_files", "products_feed.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "xml_files", "products_feed.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "xml_files", "products_feed.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "xml_files", "products_feed.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "products_feed.xml"),
                    "xml_files/products_feed.xml",
                    "products_feed.xml"
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
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Критическая ошибка при подключении к базе данных: {Message}", ex.Message);
        logger.LogError("Проверьте настройки переменных окружения: DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD");
    }
}
// --------------------------------------------

app.Run();
