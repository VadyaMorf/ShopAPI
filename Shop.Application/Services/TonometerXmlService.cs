using Shop.Core.Models;
using System.Xml.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Shop.Application.Services
{
    public class TonometerXmlService
    {
        private readonly ILogger<TonometerXmlService> _logger;
        private readonly XNamespace _gNamespace = "http://base.google.com/ns/1.0";
        
        // Маппинг категорий к именам файлов
        private readonly Dictionary<string, string> _categoryFileMapping = new()
        {
            { "tonometrs", "tonometrs_catalog" },
            { "glukometrs", "glukometrs" },
            { "med_foots", "med_foots" },
            { "orto_foots", "orto_foots" },
            { "bandajis", "bandajis" },
            { "massagers", "massagers" },
            { "pillows", "pillows" },
            { "stelki", "stelki" },
            { "trikotaj", "trikotaj" },
            { "steps", "steps" },
            { "grelki", "grelki" },
            { "weights", "weights" },
            { "applicators", "applicators" },
            { "bactosfera", "bactosfera" },
            { "tools_for_medecine", "tools_for_medecine" },
            { "hear_apparats", "hear_apparats" },
            { "irigators", "irigators" },
            { "zapchastti_for_nebulaizers", "zapchastti_for_nebulaizers" },
            { "ingalators", "ingalators" },
            { "zapchasti_for_tonometrs", "zapchasti_for_tonometrs" }
        };
        
        public TonometerXmlService(ILogger<TonometerXmlService> logger)
        {
            _logger = logger;
        }

        public async Task<List<Product>> GetTonometersFromXml(string xmlPath)
        {
            try
            {
                _logger.LogInformation("Загружаем тонометры из XML файла: {XmlPath}", xmlPath);
                
                if (!File.Exists(xmlPath))
                {
                    _logger.LogWarning("XML файл не найден: {XmlPath}", xmlPath);
                    return new List<Product>();
                }

                var xmlContent = await File.ReadAllTextAsync(xmlPath);
                var xdoc = XDocument.Parse(xmlContent);
                
                var tonometers = new List<Product>();
                var items = xdoc.Descendants("item").ToList();
                
                _logger.LogInformation("Найдено {ItemsCount} товаров в XML", items.Count);
                
                foreach (var item in items)
                {
                    try
                    {
                        var product = ParseTonometerFromXml(item);
                        if (product != null)
                        {
                            tonometers.Add(product);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при парсинге товара: {Message}", ex.Message);
                    }
                }
                
                _logger.LogInformation("Успешно загружено {TonometersCount} тонометров", tonometers.Count);
                return tonometers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке тонометров из XML: {Message}", ex.Message);
                return new List<Product>();
            }
        }

        private Product? ParseTonometerFromXml(XElement item)
        {
            try
            {
                var id = long.TryParse(item.Element(_gNamespace + "id")?.Value, out var productId) ? productId : 0;
                if (id == 0) return null;

                var title = item.Element(_gNamespace + "title")?.Value ?? string.Empty;
                var description = item.Element(_gNamespace + "description")?.Value ?? string.Empty;
                var formattedDescription = FormatDescription(description);
                var url = item.Element(_gNamespace + "link")?.Value ?? string.Empty;
                var imageLink = item.Element(_gNamespace + "image_link")?.Value ?? string.Empty;
                var availability = item.Element(_gNamespace + "availability")?.Value == "in stock";
                var priceText = item.Element(_gNamespace + "price")?.Value ?? "0";
                var brand = item.Element(_gNamespace + "brand")?.Value ?? string.Empty;
                var productType = item.Element(_gNamespace + "product_type")?.Value ?? string.Empty;

                // Улучшенный парсинг цены
                var price = ParsePrice(priceText);

                // Получаем дополнительные изображения
                var additionalImages = item.Elements(_gNamespace + "additional_image_link")
                    .Select(x => x.Value)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                var pictures = new List<string> { imageLink };
                pictures.AddRange(additionalImages);

                // Парсим параметры товара
                var paramsDict = new Dictionary<string, string>();
                var productDetails = item.Elements(_gNamespace + "product_detail");
                foreach (var detail in productDetails)
                {
                    var name = detail.Element(_gNamespace + "attribute_name")?.Value ?? string.Empty;
                    var value = detail.Element(_gNamespace + "attribute_value")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        paramsDict[name] = value;
                    }
                }

                // Определяем страну производителя
                var countryOfOrigin = paramsDict.GetValueOrDefault("Країна виробник", "");

                return new Product
                {
                    Id = id,
                    Name = title,
                    Description = formattedDescription,
                    Price = price,
                    Vendor = brand,
                    CountryOfOrigin = countryOfOrigin,
                    Url = url,
                    CategoryId = 1, // ID для категории тонометров
                    Category = "Тонометры",
                    CurrencyId = "UAH",
                    Pictures = pictures,
                    Available = availability,
                    Params = paramsDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге тонометра: {Message}", ex.Message);
                return null;
            }
        }

        private decimal ParsePrice(string priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
                return 0m;

            try
            {
                // Убираем все пробелы и приводим к нижнему регистру
                var cleanPrice = priceText.Trim().ToLowerInvariant();
                
                // Используем регулярное выражение для извлечения числа
                var match = Regex.Match(cleanPrice, @"(\d+(?:[.,]\d+)?)");
                if (match.Success)
                {
                    var numberStr = match.Groups[1].Value;
                    
                    // Заменяем запятую на точку для корректного парсинга
                    numberStr = numberStr.Replace(',', '.');
                    
                    // Используем InvariantCulture для парсинга с точкой как разделителем
                    if (decimal.TryParse(numberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    {
                        return result;
                    }
                }
                
                return 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге цены '{PriceText}': {Message}", priceText, ex.Message);
                return 0m;
            }
        }

        private string FormatDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            try
            {
                var techBlock = new List<string>();
                var mainBlock = new List<string>();
                var inTech = false;

                // Разбиваем на строки по точке и переносам
                var lines = description.Replace("\r", "").Split(new[] {'.', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

                foreach (var line in lines)
                {
                    if (line.Contains("Технічні характеристики"))
                    {
                        inTech = true;
                        continue;
                    }
                    if (inTech)
                    {
                        // Технические характеристики: ищем двоеточие и делаем маркированный список
                        if (line.Contains(":"))
                        {
                            var clean = line.Replace("є", "✓ есть").Replace("немає", "✗ нет");
                            techBlock.Add($"- {clean}");
                        }
                        else
                        {
                            techBlock.Add($"- {line}");
                        }
                    }
                    else
                    {
                        // Основной текст — просто абзацы
                        mainBlock.Add(line.Replace("є", "есть").Replace("немає", "нет"));
                    }
                }

                var result = "";
                if (mainBlock.Any())
                {
                    result += string.Join("\n\n", mainBlock);
                }
                if (techBlock.Any())
                {
                    result += "\n\n**Технические характеристики:**\n" + string.Join("\n", techBlock);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при форматировании описания: {Message}", ex.Message);
                return description;
            }
        }

        public async Task<List<Product>> GetProductsByCategory(string category)
        {
            // Получаем правильное имя файла из маппинга
            var fileName = _categoryFileMapping.GetValueOrDefault(category.ToLowerInvariant(), category);
            
            // Формируем путь к файлу по имени категории
            var xmlPath = Path.Combine("xml_files", $"{fileName}.xml");
            
            // Добавляем отладочную информацию
            var fullPath = Path.GetFullPath(xmlPath);
            _logger.LogInformation("Ищем файл: {FullPath}", fullPath);
            
            if (!File.Exists(xmlPath))
            {
                _logger.LogWarning("XML файл не найден для категории: {Category} (файл: {FileName})", category, fileName);
                return new List<Product>();
            }
            return await GetProductsFromXml(xmlPath, category);
        }

        // Универсальный метод для парсинга любого файла
        private async Task<List<Product>> GetProductsFromXml(string xmlPath, string category)
        {
            try
            {
                _logger.LogInformation("Загружаем товары из XML файла: {XmlPath}", xmlPath);
                var xmlContent = await File.ReadAllTextAsync(xmlPath);
                var xdoc = XDocument.Parse(xmlContent);
                var products = new List<Product>();
                var items = xdoc.Descendants("item").ToList();
                _logger.LogInformation("Найдено {ItemsCount} элементов item в XML", items.Count);
                
                var successCount = 0;
                foreach (var item in items)
                {
                    try
                    {
                        var product = ParseProductFromXml(item, category);
                        if (product != null)
                        {
                            products.Add(product);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при парсинге товара: {Message}", ex.Message);
                    }
                }
                _logger.LogInformation("Успешно загружено {ProductsCount} товаров из {ItemsCount} элементов", successCount, items.Count);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке товаров из XML: {Message}", ex.Message);
                return new List<Product>();
            }
        }

        // Универсальный парсер товара
        private Product? ParseProductFromXml(XElement item, string category)
        {
            try
            {
                var id = long.TryParse(item.Element(_gNamespace + "id")?.Value, out var productId) ? productId : 0;
                if (id == 0) return null;
                var title = item.Element(_gNamespace + "title")?.Value ?? string.Empty;
                var description = item.Element(_gNamespace + "description")?.Value ?? string.Empty;
                var formattedDescription = FormatDescription(description);
                var url = item.Element(_gNamespace + "link")?.Value ?? string.Empty;
                var imageLink = item.Element(_gNamespace + "image_link")?.Value ?? string.Empty;
                var availability = item.Element(_gNamespace + "availability")?.Value == "in stock";
                var priceText = item.Element(_gNamespace + "price")?.Value ?? "0";
                var brand = item.Element(_gNamespace + "brand")?.Value ?? string.Empty;
                var productType = item.Element(_gNamespace + "product_type")?.Value ?? string.Empty;
                var price = ParsePrice(priceText);
                var additionalImages = item.Elements(_gNamespace + "additional_image_link")
                    .Select(x => x.Value)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
                var pictures = new List<string> { imageLink };
                pictures.AddRange(additionalImages);
                var paramsDict = new Dictionary<string, string>();
                var productDetails = item.Elements(_gNamespace + "product_detail");
                foreach (var detail in productDetails)
                {
                    var name = detail.Element(_gNamespace + "attribute_name")?.Value ?? string.Empty;
                    var value = detail.Element(_gNamespace + "attribute_value")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        paramsDict[name] = value;
                    }
                }
                var countryOfOrigin = paramsDict.GetValueOrDefault("Країна виробник", "");
                return new Product
                {
                    Id = id,
                    Name = title,
                    Description = formattedDescription,
                    Price = price,
                    Vendor = brand,
                    CountryOfOrigin = countryOfOrigin,
                    Url = url,
                    CategoryId = 0, // универсально, если нужно — можно добавить маппинг
                    Category = category,
                    CurrencyId = "UAH",
                    Pictures = pictures,
                    Available = availability,
                    Params = paramsDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге товара: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<List<Product>> GetProductsFromFeed()
        {
            var xmlPath = Path.Combine("xml_files", "products_feed.xml");
            
            // Добавляем отладочную информацию
            var fullPath = Path.GetFullPath(xmlPath);
            _logger.LogInformation("Ищем файл products_feed: {FullPath}", fullPath);
            
            if (!File.Exists(xmlPath))
            {
                _logger.LogWarning("Файл products_feed.xml не найден по пути: {Path}", xmlPath);
                return new List<Product>();
            }
            
            _logger.LogInformation("Файл найден, начинаем парсинг YML");
            var products = await GetProductsFromYmlFeed(xmlPath);
            _logger.LogInformation("Парсинг завершен, найдено товаров: {Count}", products.Count);
            
            return products;
        }

        // Специальный метод для парсинга YML-файла
        private async Task<List<Product>> GetProductsFromYmlFeed(string xmlPath)
        {
            try
            {
                _logger.LogInformation("Загружаем товары из YML файла: {XmlPath}", xmlPath);
                var xmlContent = await File.ReadAllTextAsync(xmlPath);
                var xdoc = XDocument.Parse(xmlContent);
                var products = new List<Product>();
                
                // В YML-файле товары находятся в элементах <offer>
                var offers = xdoc.Descendants("offer").ToList();
                _logger.LogInformation("Найдено {OffersCount} элементов offer в YML", offers.Count);
                
                var successCount = 0;
                foreach (var offer in offers)
                {
                    try
                    {
                        var product = ParseProductFromYmlOffer(offer);
                        if (product != null)
                        {
                            products.Add(product);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при парсинге товара из YML: {Message}", ex.Message);
                    }
                }
                _logger.LogInformation("Успешно загружено {ProductsCount} товаров из {OffersCount} элементов", successCount, offers.Count);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке товаров из YML: {Message}", ex.Message);
                return new List<Product>();
            }
        }

        // Парсер для элементов offer в YML-файле
        private Product? ParseProductFromYmlOffer(XElement offer)
        {
            try
            {
                var id = long.TryParse(offer.Attribute("id")?.Value, out var productId) ? productId : 0;
                if (id == 0) return null;

                var name = offer.Element("name")?.Value ?? string.Empty;
                var description = offer.Element("description")?.Value ?? string.Empty;
                var formattedDescription = FormatDescription(description);
                var url = offer.Element("url")?.Value ?? string.Empty;
                var priceText = offer.Element("price")?.Value ?? "0";
                var currency = offer.Element("currencyId")?.Value ?? "UAH";
                var categoryId = long.TryParse(offer.Element("categoryId")?.Value, out var catId) ? catId : 0;
                var available = offer.Attribute("available")?.Value == "true";
                var vendor = offer.Element("vendor")?.Value ?? string.Empty;

                var price = ParsePrice(priceText);

                // Получаем изображения
                var pictures = new List<string>();
                var pictureElements = offer.Elements("picture");
                foreach (var picture in pictureElements)
                {
                    var pictureUrl = picture.Value;
                    if (!string.IsNullOrEmpty(pictureUrl))
                    {
                        pictures.Add(pictureUrl);
                    }
                }

                // Парсим параметры товара
                var paramsDict = new Dictionary<string, string>();
                var paramElements = offer.Elements("param");
                foreach (var param in paramElements)
                {
                    var paramName = param.Attribute("name")?.Value ?? string.Empty;
                    var value = param.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(value))
                    {
                        paramsDict[paramName] = value;
                    }
                }

                return new Product
                {
                    Id = id,
                    Name = name,
                    Description = formattedDescription,
                    Price = price,
                    Vendor = vendor,
                    CountryOfOrigin = paramsDict.GetValueOrDefault("Країна виробник", ""),
                    Url = url,
                    CategoryId = categoryId,
                    Category = "Все товары",
                    CurrencyId = currency,
                    Pictures = pictures,
                    Available = available,
                    Params = paramsDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге товара из YML offer: {Message}", ex.Message);
                return null;
            }
        }
    }
} 