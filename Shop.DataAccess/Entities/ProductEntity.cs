using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DataAccess.Entities
{
    public class ProductEntity
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string CurrencyId { get; set; } = string.Empty;
        public string Pictures { get; set; } = string.Empty; // JSON-массив
        public bool Available { get; set; }
        public string Params { get; set; } = string.Empty; // JSON-словарь
    }
}
