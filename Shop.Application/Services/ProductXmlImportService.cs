using Shop.Core.Models;
using Shop.DataAccess;
using Shop.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Shop.Application.Services
{
    public class ProductXmlImportService
    {
        private readonly ShopDbContext _context;
        private readonly ILogger<ProductXmlImportService> _logger;
        
        public ProductXmlImportService(ShopDbContext context, ILogger<ProductXmlImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ImportAll(string xmlPath)
        {
            try
            {
                _logger.LogInformation("Начинаем импорт данных из файла: {XmlPath}", xmlPath);
                
                // Удаляем все старые товары
                var deletedCount = await _context.Products.ExecuteDeleteAsync();
                _logger.LogInformation("Удалено {DeletedCount} старых товаров", deletedCount);

                // Загружаем XML с правильной кодировкой
                var xmlContent = await File.ReadAllTextAsync(xmlPath, Encoding.UTF8);
                var xdoc = XDocument.Parse(xmlContent);
                
                // Создаем словарь категорий
                var categories = xdoc.Descendants("category")
                    .ToDictionary(
                        x => long.TryParse(x.Attribute("id")?.Value, out var id) ? id : 0,
                        x => x.Value
                    );
                
                _logger.LogInformation("Найдено {CategoriesCount} категорий", categories.Count);

                var offers = xdoc.Descendants("offer").ToList();
                _logger.LogInformation("Найдено {OffersCount} товаров", offers.Count);
                
                var products = new List<ProductEntity>();
                var processedCount = 0;
                
                foreach (var offer in offers)
                {
                    try
                    {
                        var categoryId = long.TryParse(offer.Element("categoryId")?.Value, out var cat) ? cat : 0;
                        var categoryName = categories.ContainsKey(categoryId) ? categories[categoryId] : string.Empty;
                        
                        var pictures = offer.Elements("picture").Select(x => x.Value).ToList();
                        var paramDict = offer.Elements("param")
                            .ToDictionary(
                                x => x.Attribute("name")?.Value ?? string.Empty,
                                x => x.Value
                            );
                            
                        var product = new ProductEntity
                        {
                            Id = long.TryParse(offer.Attribute("id")?.Value, out var id) ? id : 0,
                            Name = offer.Element("name")?.Value ?? string.Empty,
                            Description = offer.Element("description")?.Value ?? string.Empty,
                            Price = decimal.TryParse(offer.Element("price")?.Value, out var price) ? price : 0,
                            Vendor = offer.Element("vendor")?.Value ?? string.Empty,
                            CountryOfOrigin = offer.Element("country_of_origin")?.Value ?? string.Empty,
                            Url = offer.Element("url")?.Value ?? string.Empty,
                            CategoryId = categoryId,
                            Category = categoryName,
                            CurrencyId = offer.Element("currencyId")?.Value ?? string.Empty,
                            Pictures = JsonSerializer.Serialize(pictures),
                            Available = offer.Attribute("available")?.Value == "true",
                            Params = JsonSerializer.Serialize(paramDict)
                        };
                        
                        products.Add(product);
                        processedCount++;
                        
                        if (processedCount % 100 == 0)
                        {
                            _logger.LogInformation("Обработано {ProcessedCount} товаров", processedCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке товара {OfferId}: {Message}", 
                            offer.Attribute("id")?.Value, ex.Message);
                    }
                }
                
                _logger.LogInformation("Добавляем {ProductsCount} товаров в базу данных", products.Count);
                
                await _context.Products.AddRangeAsync(products);
                var savedCount = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Успешно сохранено {SavedCount} товаров в базу данных", savedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при импорте данных: {Message}", ex.Message);
                throw;
            }
        }
    }
} 