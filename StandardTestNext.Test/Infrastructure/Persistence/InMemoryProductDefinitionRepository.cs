using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryProductDefinitionRepository : IProductDefinitionRepository
{
    private readonly Dictionary<string, ProductDefinition> _products = new(StringComparer.OrdinalIgnoreCase);

    public Task<ProductDefinition?> FindByKindAsync(string productKind, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(productKind, out var product);
        return Task.FromResult(product);
    }

    public Task SaveAsync(ProductDefinition product, CancellationToken cancellationToken = default)
    {
        _products[product.ProductKind] = product;
        return Task.CompletedTask;
    }
}
