namespace Shop.Contracts
{
    public record ProductsFilterResponse(
        long Id, 
        string Name, 
        string Description, 
        decimal Price, 
        string Vendor,
        string CountryOfOrigin,
        string Url,
        long CategoryId,
        string Category,
        string CurrencyId,
        List<string> Pictures,
        bool Available,
        Dictionary<string, string> Params
    );
} 