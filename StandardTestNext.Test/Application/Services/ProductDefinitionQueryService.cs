using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class ProductDefinitionQueryService : IProductDefinitionQueryService
{
    private readonly IProductDefinitionRepository _productRepository;

    public ProductDefinitionQueryService(IProductDefinitionRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<ProductDefinition?> GetByKindAsync(string productKind, CancellationToken cancellationToken = default)
    {
        return _productRepository.FindByKindAsync(productKind, cancellationToken);
    }

    public Task<IReadOnlyList<ProductDefinition>> ListRecentAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        return _productRepository.ListRecentAsync(take, cancellationToken);
    }
}
