namespace Shop.Contracts
{
    public record ProductsRequest(string Title, string Description, decimal Price ,decimal Count);
}
