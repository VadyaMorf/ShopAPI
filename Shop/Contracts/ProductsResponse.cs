namespace Shop.Contracts
{
    public record ProductsResponse(Guid id, string Title, string Description, decimal Price, decimal Count);
}
