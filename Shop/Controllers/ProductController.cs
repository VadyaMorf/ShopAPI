using Microsoft.AspNetCore.Mvc;
using Shop.Contracts;
using Shop.Core.Abstractions;
using Shop.Application.Services;
using Shop.Core.Models;
using Microsoft.Extensions.Logging;

namespace Shop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ProductXmlImportService _importService;
        private readonly TonometerXmlService _tonometerXmlService;
        private readonly ILogger<ProductsController> _logger;
        
        public ProductsController(
            IProductService productService, 
            ProductXmlImportService importService,
            TonometerXmlService tonometerXmlService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _importService = importService;
            _tonometerXmlService = tonometerXmlService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductsResponse>>> GetProducts()
        {
            var products = await _tonometerXmlService.GetProductsFromFeed();
            var response = products.Select(b => new ProductsResponse(
                b.Id,
                b.Name,
                b.Description,
                b.Price,
                b.Vendor,
                b.CountryOfOrigin,
                b.Url,
                b.CategoryId,
                b.Category,
                b.CurrencyId,
                b.Pictures,
                b.Available,
                b.Params
            ));
            return Ok(response);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<ProductsFilterResponse>>> GetProductsByCategory(string category)
        {
            var products = await _tonometerXmlService.GetProductsByCategory(category);
            var response = products.Select(b => new ProductsFilterResponse(
                b.Id,
                b.Name,
                b.Description,
                b.Price,
                b.Vendor,
                b.CountryOfOrigin,
                b.Url,
                b.CategoryId,
                b.Category,
                b.CurrencyId,
                b.Pictures,
                b.Available,
                b.Params
            ));
            return Ok(response);
        }


    }
}
