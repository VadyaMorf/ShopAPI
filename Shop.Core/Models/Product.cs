using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Shop.Core.Models
{
    public class Product
    {
        public const int Max_Titile_Lenght = 250;
        // public Guid Id { get; }
        public string Title { get; } = string.Empty;
        public decimal Count { get; }

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
        public List<string> Pictures { get; set; } = new List<string>();
        public bool Available { get; set; }
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }
}
